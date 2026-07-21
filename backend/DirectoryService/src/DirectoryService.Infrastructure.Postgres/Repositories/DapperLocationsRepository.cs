using Dapper;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Postgres.DataStorage;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

internal sealed class DapperLocationsRepository(IDbConnectionFactory dbConnectionFactory) : ILocationRepository
{
	private readonly IDbConnectionFactory _dbConnectionFactory =
		dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));

	public async Task AddAsync(Location location, CancellationToken cancellationToken = default)
	{
		using var connection = await _dbConnectionFactory.GetConnectionAsync(cancellationToken);

		var parameters = new DynamicParameters();
		parameters.Add("Name", location.Name.Value);
		parameters.Add("Id", location.Id);
		parameters.Add("CreatedAt", location.CreatedAt);
		parameters.Add("Building", location.Address.Building);
		parameters.Add("City", location.Address.City);
		parameters.Add("Country", location.Address.Country);
		parameters.Add("Street", location.Address.Street);

		await connection.ExecuteAsync("""
		                              	INSERT INTO "locations"("id", "created_at", "building", "city", "country", "street", "name")
		                              	VALUES(@Id, @CreatedAt, @Building, @City, @Country, @Street, @Name);
		                              """, parameters);
	}

	public async Task<bool> ExistWithSameNameAsync(string name, CancellationToken cancellationToken = default)
	{
		using var connection = await _dbConnectionFactory.GetConnectionAsync(cancellationToken);
		var parameters = new DynamicParameters();
		parameters.Add("Name", name.ToUpperInvariant());
		
		var result = await connection.QueryFirstAsync<int>("""
		                                        SELECT COUNT(*) FROM "locations"
		                                        WHERE UPPER(name) = @Name
		                                        """, parameters);
		
		return result > 0;
	}
}