using CSharpFunctionalExtensions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Domain.Departments;

public sealed class Department : Entity<Guid>, ISoftDeletable
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

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public static Result<Department, Error> Create(
        Guid id,
        DepartmentName name,
        DepartmentSlug slug,
        ParentInfo? parentInfo,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            return Result.Failure<Department, Error>(Error.Validation("department.id.invalid", "Id is required"));

        if (parentInfo?.Id == Guid.Empty)
            return Result.Failure<Department, Error>(Error.Validation("department.parent.id.invalid", "ParentId is required"));

        if (createdAt == default)
            return Result.Failure<Department, Error>(Error.Validation("department.createAt.invalid", "CreatedAt is required"));

        var pathResult = BuildPath(slug, parentInfo);
        if (pathResult.IsFailure)
            return Result.Failure<Department, Error>(pathResult.Error);

        return Result.Success<Department, Error>(
            new Department(id, name, slug, pathResult.Value, parentInfo?.Id, createdAt));
    }

    private static Result<DepartmentPath, Error> BuildPath(
        DepartmentSlug slug,
        ParentInfo? parentInfo)
    {
        var path = parentInfo != null ? $"{parentInfo.Path.Value}/{slug.Value}" : $"/{slug.Value}";

        return DepartmentPath.Create(path);
    }

    public UnitResult<Error> UpdateName(DepartmentName name,
        DateTime updatedAt)
    {
        if (updatedAt == default)
            return UnitResult.Failure(Error.Validation("department.updateAt.invalid", "UpdatedAt is required"));

        if (CreatedAt > updatedAt)
            return UnitResult.Failure(Error.Validation("department.updateAt.invalid", "UpdatedAt must be greater than CreatedAt"));

        if (CreatedAt == updatedAt)
            return UnitResult.Failure(Error.Validation("department.updateAt.invalid", "UpdatedAt cannot be equal to CreatedAt"));

        UpdatedAt = updatedAt;
        Name = name;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Update(
        DepartmentName name,
        ParentInfo? parentInfo,
        DateTime updatedAt)
    {
        if (updatedAt == default)
            return UnitResult.Failure(Error.Validation("department.updateAt.invalid", "UpdatedAt is required"));

        if (CreatedAt > updatedAt)
            return UnitResult.Failure(Error.Validation("department.updateAt.invalid", "UpdatedAt must be greater than CreatedAt"));

        if (CreatedAt == updatedAt)
            return UnitResult.Failure(Error.Validation("department.updateAt.invalid", "UpdatedAt cannot be equal to CreatedAt"));

        if (parentInfo != null && parentInfo.Id == Guid.Empty)
        {
            return UnitResult.Failure(Error.Validation("parentInfo.id.invalid", "ParentId is required"));
        }

        var pathResult = BuildPath(Slug, parentInfo);
        if (pathResult.IsFailure)
            return UnitResult.Failure(pathResult.Error);

        Path = pathResult.Value;
        ParentId = parentInfo?.Id;
        UpdatedAt = updatedAt;
        Name = name;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete(DateTime deletedAt)
    {
        if (deletedAt == default || deletedAt < CreatedAt)
            return UnitResult.Failure(Error.Validation("department.deletedAt.invalid", "DeletedAt is invalid"));

        if (IsDeleted)
            return UnitResult.Failure(Error.Conflict("department.already.deleted", "Подразделение уже удалено"));

        IsDeleted = true;
        DeletedAt = deletedAt;
        return UnitResult.Success<Error>();
    }
}
