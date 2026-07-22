using DirectoryService.Contracts.Departments;

namespace DirectoryService.Core.Departments;

public interface IDepartmentService
{
	Task<Guid> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default);
}