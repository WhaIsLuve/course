using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Extensions;
using DirectoryService.SharedKernel.Errors;
using FluentValidation;
using FluentValidation.Results;

namespace DirectoryService.Core.Decorators;

public sealed class CommandValidationHandlerDecorator<TCommand, TResponse>(
	ICommandHandler<TCommand, TResponse> decorated,
	IEnumerable<IValidator<TCommand>> validators)
	: ICommandHandler<TCommand, TResponse>
	where TCommand : ICommand<TResponse>
{
	private readonly ICommandHandler<TCommand, TResponse> _decorated =
		decorated ?? throw new ArgumentNullException(nameof(decorated));
	private readonly IValidator<TCommand>[] _validators =
		(validators ?? throw new ArgumentNullException(nameof(validators))).ToArray();

	public async Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
	{
		if (_validators.Length == 0)
			return await _decorated.HandleAsync(command, cancellationToken);

		var failures = new List<ValidationFailure>();
		foreach (var validator in _validators)
		{
			var result = await validator.ValidateAsync(command, cancellationToken);
			if (!result.IsValid)
				failures.AddRange(result.Errors);
		}

		return failures.Count == 0
			? await _decorated.HandleAsync(command, cancellationToken)
			: command.CreateFailure(Error.Validation(new ValidationResult(failures).ToErrorMessages()));
	}
}
