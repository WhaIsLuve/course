using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.DepartmentPositions;

public sealed class DepartmentPosition : Entity<Guid>
{
    private DepartmentPosition(Guid id, Guid departmentId, Guid positionId, DateTime createdAt) : base(id)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
        CreatedAt = createdAt;
    }

    public Guid DepartmentId { get; }

    public Guid PositionId { get; }

    public DateTime CreatedAt { get; }

    public static Result<DepartmentPosition, string> Create(
        Guid id,
        Guid departmentId,
        Guid positionId,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            return Result.Failure<DepartmentPosition, string>("Id cannot be empty");

        if (departmentId == Guid.Empty)
            return Result.Failure<DepartmentPosition, string>("DepartmentId cannot be empty");

        if (positionId == Guid.Empty)
            return Result.Failure<DepartmentPosition, string>("PositionId cannot be empty");

        if (createdAt == default)
            return Result.Failure<DepartmentPosition, string>("CreatedAt is required");

        return Result.Success<DepartmentPosition, string>(
            new DepartmentPosition(id, departmentId, positionId, createdAt));
    }
}