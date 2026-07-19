namespace DirectoryService.Contracts.Departments;

public record CreateDepartmentDto(string Name, string Slug, string Path, Guid? ParentId);