namespace DirectoryService.Contracts.Departments;

public sealed record DepartmentResponse(
    Guid Id,
    string Name,
    string Slug,
    string Path,
    Guid? ParentId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
