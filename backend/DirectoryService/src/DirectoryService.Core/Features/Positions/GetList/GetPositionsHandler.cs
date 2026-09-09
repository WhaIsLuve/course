using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Positions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Domain.Positions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Core.Features.Positions.GetList;

public sealed class GetPositionsHandler(IReadDbContext readDbContext)
    : IQueryHandler<GetPositionsQuery, Result<PagedResult<PositionListItemDto>, Error>>
{
    private const int MaxPageSize = 100;
    private const int MaxSearchLength = 200;

    private readonly IReadDbContext _readDbContext =
        readDbContext ?? throw new ArgumentNullException(nameof(readDbContext));

    public async Task<Result<PagedResult<PositionListItemDto>, Error>> HandleAsync(
        GetPositionsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var sortBy = query.SortBy ?? "name";
        var sortDir = query.SortDir ?? "asc";
        var validationError = Validate(query, sortBy, sortDir);
        if (validationError is not null)
            return validationError;

        int skip;
        try
        {
            skip = checked((query.Page - 1) * query.PageSize);
        }
        catch (OverflowException)
        {
            return Error.Validation(
                "positions.page.invalid",
                "Номер страницы слишком велик для выбранного размера страницы",
                "page");
        }

        var filteredPositions = _readDbContext.Positions;
        if (!string.IsNullOrEmpty(query.Search))
        {
            var searchPattern = $"%{EscapeLikePattern(query.Search)}%".ToUpperInvariant();
#pragma warning disable CA1304, CA1311, MA0011
            filteredPositions = filteredPositions.Where(position =>
                EF.Functions.Like(position.Name.Value.ToUpper(), searchPattern, "\\"));
#pragma warning restore CA1304, CA1311, MA0011
        }

        var totalCount = await filteredPositions.CountAsync(cancellationToken);

        IOrderedQueryable<Position> orderedPositions;
        if (string.Equals(sortBy, "name", StringComparison.Ordinal))
        {
            orderedPositions = string.Equals(sortDir, "asc", StringComparison.Ordinal)
                ? filteredPositions.OrderBy(position => position.Name.Value)
                : filteredPositions.OrderByDescending(position => position.Name.Value);
        }
        else
        {
            orderedPositions = string.Equals(sortDir, "asc", StringComparison.Ordinal)
                ? filteredPositions.OrderBy(position => position.CreatedAt)
                : filteredPositions.OrderByDescending(position => position.CreatedAt);
        }

        var items = await orderedPositions
            .ThenBy(position => position.Id)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(position => new PositionListItemDto(
                position.Id,
                position.Name.Value,
                position.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<PositionListItemDto>(items, totalCount, query.Page, query.PageSize);
    }

    private static Error? Validate(GetPositionsQuery query, string sortBy, string sortDir)
    {
        if (query.Page < 1)
            return Error.Validation("positions.page.invalid", "Номер страницы должен начинаться с 1", "page");

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
            return Error.Validation(
                "positions.page.size.invalid",
                $"Размер страницы должен быть от 1 до {MaxPageSize}",
                "pageSize");

        if (query.Search is not null && query.Search.Length > MaxSearchLength)
            return Error.Validation(
                "positions.search.too.long",
                $"Поисковая строка не может быть длиннее {MaxSearchLength} символов",
                "search");

        if (sortBy is not ("name" or "createdAt"))
            return Error.Validation(
                "positions.sort.by.invalid",
                "Поле сортировки должно быть name или createdAt",
                "sortBy");

        if (sortDir is not ("asc" or "desc"))
            return Error.Validation(
                "positions.sort.direction.invalid",
                "Направление сортировки должно быть asc или desc",
                "sortDir");

        return null;
    }

    private static string EscapeLikePattern(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
