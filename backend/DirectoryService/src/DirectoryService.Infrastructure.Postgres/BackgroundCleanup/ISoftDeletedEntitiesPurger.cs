namespace DirectoryService.Infrastructure.Postgres.BackgroundCleanup;

public interface ISoftDeletedEntitiesPurger
{
    Task<int> PurgeAsync(CancellationToken cancellationToken = default);
}
