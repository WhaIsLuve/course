using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Decorators;

public sealed class CommandTransactionHandlerDecorator<TCommand, TResponse>(
	ICommandHandler<TCommand, TResponse> decorated,
	ITransactionManager transactionManager)
	: ICommandHandler<TCommand, TResponse>
	where TCommand : ICommand<TResponse>
{
	private readonly ICommandHandler<TCommand, TResponse> _decorated =
		decorated ?? throw new ArgumentNullException(nameof(decorated));
	private readonly ITransactionManager _transactionManager =
		transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));

	public async Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
	{
		var transactional = command is ITransactionalCommand<TResponse>;
		if (transactional)
		{
			var begin = await _transactionManager.BeginTransactionAsync(cancellationToken);
			if (begin.IsFailure)
				return command.CreateFailure(begin.Error);
		}

		TResponse response;
		var handlerCompleted = false;
		try
		{
			response = await _decorated.HandleAsync(command, cancellationToken);
			handlerCompleted = true;
		}
		catch (Exception exception) when (exception is not OperationCanceledException)
		{
			var error = _transactionManager.TryMapDatabaseException(exception);
			if (error is null)
				throw;

			return command.CreateFailure(error);
		}
		finally
		{
			if (transactional && !handlerCompleted)
				await _transactionManager.RollbackTransactionAsync(CancellationToken.None);
		}

		if (command.IsFailure(response))
		{
			if (transactional)
			{
				var rollback = await _transactionManager.RollbackTransactionAsync(CancellationToken.None);
				if (rollback.IsFailure)
					return command.CreateFailure(rollback.Error);
			}

			return response;
		}

		var save = await _transactionManager.SaveChangesAsync(cancellationToken);
		if (save.IsFailure)
		{
			if (transactional)
				await _transactionManager.RollbackTransactionAsync(CancellationToken.None);

			return command.CreateFailure(save.Error);
		}

		if (!transactional)
			return response;

		var commit = await _transactionManager.CommitTransactionAsync(cancellationToken);
		return commit.IsSuccess ? response : command.CreateFailure(commit.Error);
	}
}
