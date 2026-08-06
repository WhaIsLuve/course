using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Departments;

public interface IDepartmentService
{
	Task<Result<Guid, Error>> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default);

	Task<UnitResult<Error>> UpdateNameAsync(Guid id, UpdateDepartmentNameDto dto, CancellationToken cancellationToken = default);

	Task<UnitResult<Error>> AttachLocation(Guid departmentId, Guid locationId, CancellationToken cancellationToken = default);
	Task<UnitResult<Error>> DetachLocation(Guid departmentId, Guid locationId, CancellationToken cancellationToken = default);
}