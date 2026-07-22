using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;

namespace DirectoryService.Core.Departments;

public interface IDepartmentRepository
{
	void AddDepartment(Department department);

	void AddDepartmentLocations(IReadOnlyList<DepartmentLocation> departmentLocations);
	ValueTask<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task Save(CancellationToken cancellationToken = default);
}