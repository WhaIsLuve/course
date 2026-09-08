using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Locations.GetList;

public sealed record GetLocationsQuery(
    string? Search = null,
    int? MinDepartmentCount = null,
    string? SortBy = "name",
    string? SortDir = "asc",
    int Page = 1,
    int PageSize = 20) : IQuery<Result<PagedResult<LocationListItemDto>, Error>>;
