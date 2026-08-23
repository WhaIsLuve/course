using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Positions;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Positions.Rename;

public sealed record RenamePositionCommand(Guid PositionId, UpdatePositionDto Dto)
    : ITransactionalCommand<UnitResult<Error>>
{
    public UnitResult<Error> CreateFailure(Error failure) => UnitResult.Failure(failure);

    public bool IsFailure(UnitResult<Error> response) => response.IsFailure;
}
