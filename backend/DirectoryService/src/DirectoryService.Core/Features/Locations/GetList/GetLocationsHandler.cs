using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Locations.GetList;

public sealed class GetLocationsHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetLocationsQuery, Result<PagedResult<LocationListItemDto>, Error>>
{
    private const int MaxPageSize = 100;
    private const int MaxSearchLength = 200;
    private const string LikeEscapeCharacter = "~";

    private readonly IDbConnectionFactory _dbConnectionFactory =
        dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));

    public async Task<Result<PagedResult<LocationListItemDto>, Error>> HandleAsync(
        GetLocationsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var sortBy = query.SortBy ?? "name";
        var sortDir = query.SortDir ?? "asc";

        var validationError = Validate(query, sortBy, sortDir);
        if (validationError is not null)
            return validationError;

        int offset;
        try
        {
            offset = checked((query.Page - 1) * query.PageSize);
        }
        catch (OverflowException)
        {
            return Error.Validation(
                "locations.page.invalid",
                "Номер страницы слишком велик для выбранного размера страницы",
                "page");
        }

        var searchPattern = string.IsNullOrEmpty(query.Search)
            ? null
            : $"%{EscapeLikePattern(query.Search)}%";
        var sortExpression = sortBy switch
        {
            "name" => "\"Name\"",
            "createdAt" => "\"CreatedAt\"",
            "departmentCount" => "\"DepartmentCount\"",
            _ => throw new InvalidOperationException("Sort field was not validated")
        };
        var sortDirection = string.Equals(sortDir, "asc", StringComparison.Ordinal) ? "ASC" : "DESC";

        var sql = $"""
            WITH filtered_locations AS (
                SELECT
                    l.id AS "Id",
                    l.name AS "Name",
                    l.country AS "Country",
                    l.city AS "City",
                    l.street AS "Street",
                    l.building AS "Building",
                    l.created_at AS "CreatedAt",
                    COUNT(DISTINCT dl.department_id)::int AS "DepartmentCount"
                FROM locations AS l
                LEFT JOIN department_locations AS dl ON dl.location_id = l.id
                WHERE @SearchPattern IS NULL
                   OR l.name ILIKE @SearchPattern ESCAPE '~'
                GROUP BY l.id, l.name, l.country, l.city, l.street, l.building, l.created_at
                HAVING @MinDepartmentCount IS NULL
                    OR COUNT(DISTINCT dl.department_id) >= @MinDepartmentCount
            ),
            total_count AS (
                SELECT COUNT(*)::int AS "TotalCount"
                FROM filtered_locations
            ),
            paged_locations AS (
                SELECT *
                FROM filtered_locations
                ORDER BY {sortExpression} {sortDirection}, "Id" ASC
                LIMIT @Limit OFFSET @Offset
            )
            SELECT
                total_count."TotalCount",
                paged_locations."Id",
                paged_locations."Name",
                paged_locations."Country",
                paged_locations."City",
                paged_locations."Street",
                paged_locations."Building",
                paged_locations."CreatedAt",
                paged_locations."DepartmentCount"
            FROM total_count
            LEFT JOIN paged_locations ON TRUE;
            """;

        using var connection = await _dbConnectionFactory.GetConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            sql,
            new
            {
                SearchPattern = searchPattern,
                MinDepartmentCount = query.MinDepartmentCount,
                Limit = query.PageSize,
                Offset = offset
            },
            cancellationToken: cancellationToken);

        var rows = (await connection.QueryAsync<LocationListQueryRow>(command)).AsList();
        var totalCount = rows.Count == 0 ? 0 : rows[0].TotalCount;
        var items = rows
            .Where(row => row.Id.HasValue)
            .Select(row => new LocationListItemDto(
                row.Id!.Value,
                row.Name!,
                row.Country!,
                row.City!,
                row.Street,
                row.Building,
                row.CreatedAt!.Value,
                row.DepartmentCount!.Value))
            .ToArray();

        return new PagedResult<LocationListItemDto>(items, totalCount, query.Page, query.PageSize);
    }

    private static Error? Validate(GetLocationsQuery query, string sortBy, string sortDir)
    {
        if (query.Page < 1)
            return Error.Validation("locations.page.invalid", "Номер страницы должен начинаться с 1", "page");

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
            return Error.Validation(
                "locations.page.size.invalid",
                $"Размер страницы должен быть от 1 до {MaxPageSize}",
                "pageSize");

        if (query.Search is not null && query.Search.Length > MaxSearchLength)
            return Error.Validation(
                "locations.search.too.long",
                $"Поисковая строка не может быть длиннее {MaxSearchLength} символов",
                "search");

        if (query.MinDepartmentCount is < 0)
            return Error.Validation(
                "locations.department.count.invalid",
                "Минимальное количество подразделений не может быть отрицательным",
                "minDepartmentCount");

        if (sortBy is not ("name" or "createdAt" or "departmentCount"))
            return Error.Validation(
                "locations.sort.by.invalid",
                "Поле сортировки должно быть name, createdAt или departmentCount",
                "sortBy");

        if (sortDir is not ("asc" or "desc"))
            return Error.Validation(
                "locations.sort.direction.invalid",
                "Направление сортировки должно быть asc или desc",
                "sortDir");

        return null;
    }

    private static string EscapeLikePattern(string value) =>
        value.Replace(LikeEscapeCharacter, LikeEscapeCharacter + LikeEscapeCharacter, StringComparison.Ordinal)
            .Replace("%", LikeEscapeCharacter + "%", StringComparison.Ordinal)
            .Replace("_", LikeEscapeCharacter + "_", StringComparison.Ordinal);

    private sealed record LocationListQueryRow(
        int TotalCount,
        Guid? Id,
        string? Name,
        string? Country,
        string? City,
        string? Street,
        string? Building,
        DateTime? CreatedAt,
        int? DepartmentCount);
}
