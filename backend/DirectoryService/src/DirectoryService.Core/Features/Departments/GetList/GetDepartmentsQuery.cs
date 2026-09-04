using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Departments.GetList;

public sealed record GetDepartmentsQuery(
    string? Search = null,
    string? SortBy = "name",
    string? SortDir = "asc",
    int Page = 1,
    int PageSize = 20) : IQuery<Result<PagedResult<DepartmentListItemDto>, Error>>;
