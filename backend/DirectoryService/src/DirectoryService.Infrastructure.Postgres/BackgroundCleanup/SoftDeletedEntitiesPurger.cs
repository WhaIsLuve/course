using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Postgres.BackgroundCleanup;

public sealed class SoftDeletedEntitiesPurger(
    AppDbContext dbContext,
    IOptions<SoftDeleteCleanupOptions> options,
    TimeProvider timeProvider) : ISoftDeletedEntitiesPurger
{
    private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IOptions<SoftDeleteCleanupOptions> _options =
        options ?? throw new ArgumentNullException(nameof(options));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

    public async Task<int> PurgeAsync(CancellationToken cancellationToken = default)
    {
        var cleanupOptions = _options.Value;
        var cutoff = _timeProvider.GetUtcNow().UtcDateTime - cleanupOptions.RetentionPeriod;
        var deletedCount = 0;

        deletedCount += await PurgeBatchesAsync(_dbContext.Locations, cutoff, cleanupOptions.BatchSize, cancellationToken);
        deletedCount += await PurgeBatchesAsync(_dbContext.Departments, cutoff, cleanupOptions.BatchSize, cancellationToken);
        deletedCount += await PurgeBatchesAsync(_dbContext.Positions, cutoff, cleanupOptions.BatchSize, cancellationToken);

        return deletedCount;
    }

    private static async Task<int> PurgeBatchesAsync<TEntity>(
        DbSet<TEntity> entities,
        DateTime cutoff,
        int batchSize,
        CancellationToken cancellationToken)
        where TEntity : class, ISoftDeletable
    {
        var totalDeleted = 0;
        int deletedInBatch;
        do
        {
            deletedInBatch = await entities
                .IgnoreQueryFilters()
                .Where(entity => entity.IsDeleted
                    && entity.DeletedAt.HasValue
                    && entity.DeletedAt.Value < cutoff)
                .OrderBy(entity => entity.DeletedAt)
                .Take(batchSize)
                .ExecuteDeleteAsync(cancellationToken);
            totalDeleted += deletedInBatch;
        }
        while (deletedInBatch == batchSize);

        return totalDeleted;
    }
}
