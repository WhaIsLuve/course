using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Departments.Create;

public sealed record CreateDepartmentCommand(CreateDepartmentDto Dto) : ICommand<Result<Guid, Error>>;
