namespace DirectoryService.Contracts.Departments;

public record UpdateDepartmentDto(string Name, string Path, Guid? ParentId);