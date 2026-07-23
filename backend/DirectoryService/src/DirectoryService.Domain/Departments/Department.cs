using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments;

public sealed class Department : Entity<Guid>
{
	private Department(Guid id, DepartmentName name, DepartmentSlug slug, DepartmentPath path, Guid? parentId,
		DateTime createdAt) : base(id)
	{
		Name = name;
		Slug = slug;
		Path = path;
		ParentId = parentId;
		CreatedAt = createdAt;
	}

	private Department()
	{
	}

	public DepartmentName Name { get; private set; } = null!;

	public DepartmentSlug Slug { get; } = null!;

	public DepartmentPath Path { get; private set; } = null!;

	public Guid? ParentId { get; private set; }

	public DateTime CreatedAt { get; }

	public DateTime? UpdatedAt { get; private set; }

	public static Result<Department, string> Create(
		Guid id,
		DepartmentName name,
		DepartmentSlug slug,
		ParentInfo? parentInfo,
		DateTime createdAt)
	{
		if (id == Guid.Empty)
			return Result.Failure<Department, string>("Id cannot be empty");

		if (parentInfo?.Id == Guid.Empty)
			return Result.Failure<Department, string>("ParentId cannot be empty");

		if (createdAt == default)
			return Result.Failure<Department, string>("CreatedAt is required");

		var pathResult = BuildPath(slug, parentInfo);
		if (pathResult.IsFailure)
			return Result.Failure<Department, string>(pathResult.Error);

		return Result.Success<Department, string>(
			new Department(id, name, slug, pathResult.Value, parentInfo?.Id, createdAt));
	}

	private static Result<DepartmentPath, string> BuildPath(
		DepartmentSlug slug,
		ParentInfo? parentInfo)
	{
		var path = parentInfo != null ? $"{parentInfo.Path.Value}/{slug.Value}" : $"/{slug.Value}";

		return DepartmentPath.Create(path);
	}

	public UnitResult<string> Update(
		DepartmentName name,
		ParentInfo? parentInfo,
		DateTime updatedAt)
	{
		if (updatedAt == default)
			return UnitResult.Failure("UpdatedAt is required");

		if (CreatedAt > updatedAt)
			return UnitResult.Failure("UpdatedAt must be greater than CreatedAt");

		if (CreatedAt == updatedAt)
			return UnitResult.Failure("UpdatedAt cannot be equal to CreatedAt");

		if (parentInfo != null && parentInfo.Id != Guid.Empty)
		{
			return UnitResult.Failure("ParentId cannot be empty");
		}

		var pathResult = BuildPath(Slug, parentInfo);
		if (pathResult.IsFailure)
			return UnitResult.Failure(pathResult.Error);

		Path = pathResult.Value;
		ParentId = parentInfo?.Id;
		UpdatedAt = updatedAt;
		Name = name;

		return UnitResult.Success<string>();
	}
}