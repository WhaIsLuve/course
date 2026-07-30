using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;

namespace DirectoryService.Core.Departments;

public interface IDepartmentRepository
{
	void AddDepartment(Department department);
	void AddDepartmentLocations(IReadOnlyList<DepartmentLocation> departmentLocations);
	void RemoveDepartmentLocation(DepartmentLocation departmentLocation);

	Task<DepartmentLocation?> GetDepartmentLocation(Guid departmentId, Guid locationId,
		CancellationToken cancellationToken = default);
	
	Task<bool> ExistDepartmentLocation(Guid departmentId, Guid locationId,
		CancellationToken cancellationToken = default);

	ValueTask<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task Save(CancellationToken cancellationToken = default);
}