using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Positions.Delete;

public sealed record DeletePositionCommand(Guid PositionId) : ITransactionalCommand<UnitResult<Error>>
{
    public UnitResult<Error> CreateFailure(Error failure) => UnitResult.Failure(failure);

    public bool IsFailure(UnitResult<Error> response) => response.IsFailure;
}
