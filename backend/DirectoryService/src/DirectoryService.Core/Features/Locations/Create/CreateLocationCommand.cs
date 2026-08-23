using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Locations.Create;

public sealed record CreateLocationCommand(CreateLocationDto Dto) : ICommand<Result<Guid, Error>>
{
	public Result<Guid, Error> CreateFailure(Error failure) => Result.Failure<Guid, Error>(failure);

	public bool IsFailure(Result<Guid, Error> response) => response.IsFailure;
}
