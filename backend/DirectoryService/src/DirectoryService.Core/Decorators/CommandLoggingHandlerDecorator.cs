#pragma warning disable CA1848, CA1873

using DirectoryService.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Decorators;

public sealed class CommandLoggingHandlerDecorator<TCommand, TResponse>(
	ICommandHandler<TCommand, TResponse> decorated,
	ILogger<CommandLoggingHandlerDecorator<TCommand, TResponse>> logger)
	: ICommandHandler<TCommand, TResponse>
	where TCommand : ICommand<TResponse>
{
	private readonly ICommandHandler<TCommand, TResponse> _decorated =
		decorated ?? throw new ArgumentNullException(nameof(decorated));
	private readonly ILogger<CommandLoggingHandlerDecorator<TCommand, TResponse>> _logger =
		logger ?? throw new ArgumentNullException(nameof(logger));

	public async Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
	{
		var commandName = typeof(TCommand).Name;
		_logger.LogInformation("Начата обработка команды {CommandName}", commandName);

		var response = await _decorated.HandleAsync(command, cancellationToken);
		_logger.LogInformation(
			"Завершена обработка команды {CommandName} с результатом {ResultStatus}",
			commandName,
			command.IsFailure(response) ? "Failure" : "Success");
		return response;
	}
}

#pragma warning restore CA1848, CA1873
