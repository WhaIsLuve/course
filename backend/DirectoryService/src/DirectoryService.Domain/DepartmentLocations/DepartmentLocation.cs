using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.DepartmentLocations;

public sealed class DepartmentLocation : Entity<Guid>
{
    private DepartmentLocation(Guid id, Guid departmentId, Guid locationId, DateTime createdAt) :
        base(id)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
        CreatedAt = createdAt;
    }

    private DepartmentLocation()
    {
    }

    public Guid DepartmentId { get; }

    public Guid LocationId { get; }

    public DateTime CreatedAt { get; }

    public static Result<DepartmentLocation, string> Create(
        Guid id,
        Guid departmentId,
        Guid locationId,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            return Result.Failure<DepartmentLocation, string>("Id cannot be empty");

        if (departmentId == Guid.Empty)
            return Result.Failure<DepartmentLocation, string>("DepartmentId cannot be empty");

        if (locationId == Guid.Empty)
            return Result.Failure<DepartmentLocation, string>("LocationId cannot be empty");

        if (createdAt == default)
            return Result.Failure<DepartmentLocation, string>("CreatedAt is required");

        return Result.Success<DepartmentLocation, string>(
            new DepartmentLocation(id, departmentId, locationId, createdAt));
    }
}