using DirectoryService.Contracts.Departments;

namespace DirectoryService.Core.Departments;

public interface IDepartmentService
{
	Task<Guid> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default);

	Task UpdateNameAsync(Guid id, UpdateDepartmentNameDto dto, CancellationToken cancellationToken = default);

	Task AttachLocation(Guid departmentId, Guid locationId, CancellationToken cancellationToken = default);
	Task DetachLocation(Guid departmentId, Guid locationId, CancellationToken cancellationToken = default);
}