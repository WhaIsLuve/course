using CSharpFunctionalExtensions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Domain.Locations;

public sealed class Location : Entity<Guid>, ISoftDeletable
{
	private Location(Guid id, LocationName name, Address address, DateTime createdAt) : base(id)
	{
		Name = name;
		Address = address;
		CreatedAt = createdAt;
	}

	private Location()
	{
	}

	public LocationName Name { get; private set; } = null!;

	public Address Address { get; private set; } = null!;

	public DateTime CreatedAt { get; }

	public DateTime? UpdatedAt { get; private set; }

	public bool IsDeleted { get; private set; }

	public DateTime? DeletedAt { get; private set; }

	public static Result<Location, Error> Create(
		Guid id,
		LocationName name,
		Address address,
		DateTime createdAt)
	{
		if (id == Guid.Empty)
			return Result.Failure<Location, Error>(Error.Validation("location.id.invalid", "Id cannot be empty"));

		if (createdAt == default)
			return Result.Failure<Location, Error>(Error.Validation("location.createdAt.invalid",
				"CreatedAt cannot be empty"));

		return Result.Success<Location, Error>(new Location(id, name, address, createdAt));
	}

	public UnitResult<Error> Update(
		LocationName name,
		Address address,
		DateTime updatedAt)
	{
		if (updatedAt == default)
			return UnitResult.Failure(Error.Validation("location.updateAt.invalid", "UpdatedAt is required"));

		if (CreatedAt > updatedAt)
			return UnitResult.Failure(Error.Validation("location.updateAt.invalid",
				"UpdatedAt must be greater than CreatedAt"));

		if (CreatedAt == updatedAt)
			return UnitResult.Failure(Error.Validation("location.updateAt.invalid",
				"UpdatedAt cannot be equal to CreatedAt"));


		Name = name;
		Address = address;
		UpdatedAt = updatedAt;

		return UnitResult.Success<Error>();
	}

	public UnitResult<Error> Delete(DateTime deletedAt)
	{
		if (deletedAt == default || deletedAt < CreatedAt)
			return UnitResult.Failure(Error.Validation("location.deletedAt.invalid", "DeletedAt is invalid"));

		if (IsDeleted)
			return UnitResult.Failure(Error.Conflict("location.already.deleted", "Локация уже удалена"));

		IsDeleted = true;
		DeletedAt = deletedAt;
		return UnitResult.Success<Error>();
	}
}
