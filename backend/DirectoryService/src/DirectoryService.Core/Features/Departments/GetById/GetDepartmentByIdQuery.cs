using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Departments.GetById;

public sealed record GetDepartmentByIdQuery(Guid Id) : IQuery<Result<DepartmentResponse, Error>>;
