using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Locations.Top;

public sealed class GetTopLocationHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetTopLocationQuery, Result<LocationTopResponse[], Error>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory =
        dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));

    private const string Sql = """
        SELECT
            l.id AS "Id",
            l.name AS "Name",
            l.country AS "Country",
            l.city AS "City",
            l.street AS "Street",
            l.building AS "Building",
            COUNT(DISTINCT dl.department_id)::int AS "DepartmentCount"
        FROM locations AS l
        LEFT JOIN department_locations AS dl ON dl.location_id = l.id
        GROUP BY l.id, l.name, l.country, l.city, l.street, l.building
        ORDER BY "DepartmentCount" DESC, l.name ASC, l.id ASC
        LIMIT 5;
        """;

    public async Task<Result<LocationTopResponse[], Error>> HandleAsync(
        GetTopLocationQuery query,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.GetConnectionAsync(cancellationToken);

        var command = new CommandDefinition(Sql, cancellationToken: cancellationToken);
        var locations = await connection.QueryAsync<LocationTopResponse>(command);

        return locations.ToArray();
    }
}
