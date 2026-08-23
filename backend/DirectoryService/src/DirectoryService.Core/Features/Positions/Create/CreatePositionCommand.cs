using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Positions;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Positions.Create;

public sealed record CreatePositionCommand(CreatePositionDto Dto) : ITransactionalCommand<Result<Guid, Error>>
{
    public Result<Guid, Error> CreateFailure(Error failure) => Result.Failure<Guid, Error>(failure);

    public bool IsFailure(Result<Guid, Error> response) => response.IsFailure;
}
