using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Departments.Create;

public sealed record CreateDepartmentCommand(CreateDepartmentDto Dto)
	: ITransactionalCommand<Result<Guid, Error>>
{
	public Result<Guid, Error> CreateFailure(Error failure) => Result.Failure<Guid, Error>(failure);

	public bool IsFailure(Result<Guid, Error> response) => response.IsFailure;
}
