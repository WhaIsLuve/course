using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Departments;

public interface IDepartmentRepository
{
	void AddDepartment(Department department);
	void AddDepartmentLocations(IReadOnlyList<DepartmentLocation> departmentLocations);
	void RemoveDepartmentLocation(DepartmentLocation departmentLocation);

	Task<Result<DepartmentLocation, Error>> GetDepartmentLocation(Guid departmentId, Guid locationId,
		CancellationToken cancellationToken = default);

	Task<bool> ExistDepartmentLocation(Guid departmentId, Guid locationId,
		CancellationToken cancellationToken = default);

	ValueTask<Result<Department, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<UnitResult<Error>> Save(CancellationToken cancellationToken = default);
}