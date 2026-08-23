using CSharpFunctionalExtensions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Domain.DepartmentPositions;

public sealed class DepartmentPosition : Entity<Guid>
{
    private DepartmentPosition(Guid id, Guid departmentId, Guid positionId, DateTime createdAt) : base(id)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
        CreatedAt = createdAt;
    }

    private DepartmentPosition()
    {
    }

    public Guid DepartmentId { get; }

    public Guid PositionId { get; }

    public DateTime CreatedAt { get; }

    public static Result<DepartmentPosition, Error> Create(
        Guid id,
        Guid departmentId,
        Guid positionId,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            return Error.Validation("department.position.id.invalid", "Id is required");

        if (departmentId == Guid.Empty)
            return Error.Validation("department.position.department.id.invalid", "DepartmentId is required");

        if (positionId == Guid.Empty)
            return Error.Validation("department.position.position.id.invalid", "PositionId is required");

        if (createdAt == default)
            return Error.Validation("department.position.createdAt.invalid", "CreatedAt is required");

        return Result.Success<DepartmentPosition, Error>(
            new DepartmentPosition(id, departmentId, positionId, createdAt));
    }
}
