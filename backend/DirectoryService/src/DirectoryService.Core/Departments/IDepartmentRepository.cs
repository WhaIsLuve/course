using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Departments;

public interface IDepartmentRepository
{
	void AddDepartment(Department department);
	void RemoveDepartment(Department department);
	void AddDepartmentLocations(IReadOnlyList<DepartmentLocation> departmentLocations);
	void RemoveDepartmentLocation(DepartmentLocation departmentLocation);
	void AddDepartmentPosition(DepartmentPosition departmentPosition);
	void RemoveDepartmentPosition(DepartmentPosition departmentPosition);

	Task<Result<DepartmentLocation, Error>> GetDepartmentLocation(Guid departmentId, Guid locationId,
		CancellationToken cancellationToken = default);

	Task<bool> ExistDepartmentLocation(Guid departmentId, Guid locationId,
		CancellationToken cancellationToken = default);

	Task<bool> ExistDepartmentPosition(Guid departmentId, Guid positionId,
		CancellationToken cancellationToken = default);

	Task<Result<DepartmentPosition, Error>> GetDepartmentPosition(Guid departmentId, Guid positionId,
		CancellationToken cancellationToken = default);

	ValueTask<Result<Department, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
