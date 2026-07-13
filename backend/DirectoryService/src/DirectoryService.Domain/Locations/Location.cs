using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations;

public sealed class Location : Entity<Guid>
{
    private Location(Guid id, LocationName name, Address address, DateTime createdAt) : base(id)
    {
        Name = name;
        Address = address;
        CreatedAt = createdAt;
        UpdatedAt = Maybe<DateTime>.None;
    }

    private Location()
    {
    }

    public LocationName Name { get; private set; } = null!;

    public Address Address { get; private set; } = null!;

    public DateTime CreatedAt { get; }

    public Maybe<DateTime> UpdatedAt { get; private set; }

    public static Result<Location, string> Create(
        Guid id,
        LocationName name,
        Address address,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            return Result.Failure<Location, string>("Id cannot be empty");

        if (createdAt == default)
            return Result.Failure<Location, string>("CreatedAt is required");

        return Result.Success<Location, string>(new Location(id, name, address, createdAt));
    }

    public UnitResult<string> Update(
        LocationName name,
        Address address,
        DateTime updatedAt)
    {
        if (updatedAt == default)
            return UnitResult.Failure("UpdatedAt is required");

        if (CreatedAt > updatedAt)
            return UnitResult.Failure("UpdatedAt must be greater than CreatedAt");

        if (CreatedAt == updatedAt)
            return UnitResult.Failure("UpdatedAt cannot be equal to CreatedAt");

        Name = name;
        Address = address;
        UpdatedAt = updatedAt;

        return UnitResult.Success<string>();
    }
}