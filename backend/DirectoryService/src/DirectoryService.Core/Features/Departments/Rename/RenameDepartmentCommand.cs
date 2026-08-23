using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Departments.Rename;

public sealed record RenameDepartmentCommand(Guid DepartmentId, UpdateDepartmentNameDto Dto)
	: ICommand<UnitResult<Error>>
{
	public UnitResult<Error> CreateFailure(Error failure) => UnitResult.Failure(failure);

	public bool IsFailure(UnitResult<Error> response) => response.IsFailure;
}
