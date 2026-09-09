using CSharpFunctionalExtensions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Domain.Positions;

public sealed class Position : Entity<Guid>, ISoftDeletable
{
    private Position(Guid id, PositionName name, DateTime createdAt) : base(id)
    {
        Name = name;
        CreatedAt = createdAt;
    }

    private Position()
    {
    }

    public PositionName Name { get; private set; } = null!;

    public DateTime CreatedAt { get; }

    public DateTime? UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public static Result<Position, Error> Create(Guid id, PositionName name, DateTime createdAt)
    {
        if (id == Guid.Empty)
            return Error.Validation("position.id.invalid", "Id is required");

        if (createdAt == default)
            return Error.Validation("position.createdAt.invalid", "CreatedAt is required");

        return Result.Success<Position, Error>(new Position(id, name, createdAt));
    }

    public UnitResult<Error> Update(PositionName name, DateTime updatedAt)
    {
        if (updatedAt == default)
            return UnitResult.Failure(Error.Validation("position.updatedAt.invalid", "UpdatedAt is required"));

        if (CreatedAt > updatedAt)
            return UnitResult.Failure(Error.Validation("position.updatedAt.invalid", "UpdatedAt must be greater than CreatedAt"));

        if (CreatedAt == updatedAt)
            return UnitResult.Failure(Error.Validation("position.updatedAt.invalid", "UpdatedAt cannot be equal to CreatedAt"));

        Name = name;
        UpdatedAt = updatedAt;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete(DateTime deletedAt)
    {
        if (deletedAt == default || deletedAt < CreatedAt)
            return UnitResult.Failure(Error.Validation("position.deletedAt.invalid", "DeletedAt is invalid"));

        if (IsDeleted)
            return UnitResult.Failure(Error.Conflict("position.already.deleted", "Должность уже удалена"));

        IsDeleted = true;
        DeletedAt = deletedAt;
        return UnitResult.Success<Error>();
    }
}
