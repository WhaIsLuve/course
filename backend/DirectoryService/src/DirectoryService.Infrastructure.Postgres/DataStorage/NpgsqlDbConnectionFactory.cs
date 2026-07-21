using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Postgres.DataStorage;

public sealed class NpgsqlDbConnectionFactory(IConfiguration config)
	: IDbConnectionFactory, IDisposable, IAsyncDisposable
{
	private readonly NpgsqlDataSource _dataSource = new NpgsqlDataSourceBuilder(config.GetConnectionString("Postgres"))
		.UseLoggerFactory(LoggerFactory.Create(c => c.AddConsole())).Build();

	public async ValueTask<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
	{
		return await _dataSource.OpenConnectionAsync(cancellationToken);
	}

	public void Dispose()
	{
		_dataSource.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		await _dataSource.DisposeAsync();
	}
}