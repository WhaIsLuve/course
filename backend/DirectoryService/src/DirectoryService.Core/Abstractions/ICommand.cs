#pragma warning disable CA1040, S2326

using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Abstractions;

public interface ICommand<TResponse>
{
	TResponse CreateFailure(Error failure);

	bool IsFailure(TResponse response);
}

public interface ITransactionalCommand<TResponse> : ICommand<TResponse>
{
}

#pragma warning restore CA1040, S2326
