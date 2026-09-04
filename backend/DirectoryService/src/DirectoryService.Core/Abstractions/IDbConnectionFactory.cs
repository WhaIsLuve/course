using System.Data;

namespace DirectoryService.Core.Abstractions;

public interface IDbConnectionFactory
{
	ValueTask<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}