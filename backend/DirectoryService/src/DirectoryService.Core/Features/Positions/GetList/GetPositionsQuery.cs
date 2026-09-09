using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Positions;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Positions.GetList;

public sealed record GetPositionsQuery(
    string? Search = null,
    string? SortBy = "name",
    string? SortDir = "asc",
    int Page = 1,
    int PageSize = 20) : IQuery<Result<PagedResult<PositionListItemDto>, Error>>;
