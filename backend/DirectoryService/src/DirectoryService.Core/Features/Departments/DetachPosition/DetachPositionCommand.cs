using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Departments.DetachPosition;

public sealed record DetachPositionCommand(Guid DepartmentId, Guid PositionId)
    : ITransactionalCommand<UnitResult<Error>>
{
    public UnitResult<Error> CreateFailure(Error failure) => UnitResult.Failure(failure);

    public bool IsFailure(UnitResult<Error> response) => response.IsFailure;
}
