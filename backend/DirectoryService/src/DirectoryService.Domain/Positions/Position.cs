using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Positions;

public sealed class Position : Entity<Guid>
{
    private Position(Guid id, PositionName name, DateTime createdAt) : base(id)
    {
        Name = name;
        CreatedAt = createdAt;
        UpdatedAt = Maybe<DateTime>.None;
    }

    public PositionName Name { get; private set; }

    public DateTime CreatedAt { get; }

    public Maybe<DateTime> UpdatedAt { get; private set; }

    public static Result<Position, string> Create(
        Guid id,
        PositionName name,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            return Result.Failure<Position, string>("Id cannot be empty");

        if (createdAt == default)
            return Result.Failure<Position, string>("CreatedAt is required");

        return Result.Success<Position, string>(new Position(id, name, createdAt));
    }

    public UnitResult<string> Update(
        PositionName name,
        DateTime updatedAt)
    {
        if (updatedAt == default)
            return UnitResult.Failure("UpdatedAt is required");

        if (CreatedAt > updatedAt)
            return UnitResult.Failure("UpdatedAt must be greater than CreatedAt");

        if (CreatedAt == updatedAt)
            return UnitResult.Failure("UpdatedAt cannot be equal to CreatedAt");

        Name = name;
        UpdatedAt = updatedAt;

        return UnitResult.Success<string>();
    }
}