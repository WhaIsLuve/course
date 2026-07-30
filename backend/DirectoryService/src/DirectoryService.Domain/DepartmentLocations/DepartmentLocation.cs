using CSharpFunctionalExtensions;
using DirectoryService.SharedKernel.Errors;

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

    public static Result<DepartmentLocation, Error> Create(
        Guid id,
        Guid departmentId,
        Guid locationId,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            return Result.Failure<DepartmentLocation, Error>(Error.Validation("department.locations.id.invalid", "Id cannot be empty"));

        if (departmentId == Guid.Empty)
            return Result.Failure<DepartmentLocation, Error>(Error.Validation("department.location.department.id.invalid", "DepartmentId cannot be empty"));

        if (locationId == Guid.Empty)
            return Result.Failure<DepartmentLocation, Error>(Error.Validation("department.location.location.id.invalid","LocationId cannot be empty"));

        if (createdAt == default)
            return Result.Failure<DepartmentLocation, Error>(Error.Validation("department.location.createAt.invalid", "CreatedAt cannot be empty"));

        return Result.Success<DepartmentLocation, Error>(
            new DepartmentLocation(id, departmentId, locationId, createdAt));
    }
}