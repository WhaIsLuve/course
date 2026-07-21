using System.Data;

namespace DirectoryService.Infrastructure.Postgres.DataStorage;

public interface IDbConnectionFactory
{
	ValueTask<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}