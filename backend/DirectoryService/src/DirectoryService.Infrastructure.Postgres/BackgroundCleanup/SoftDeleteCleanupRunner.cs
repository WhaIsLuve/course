using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.BackgroundCleanup;

public sealed partial class SoftDeleteCleanupRunner(
    IServiceScopeFactory scopeFactory,
    ILogger<SoftDeleteCleanupRunner> logger)
{
    private readonly IServiceScopeFactory _scopeFactory =
        scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly ILogger<SoftDeleteCleanupRunner> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task RunOnceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var purger = scope.ServiceProvider.GetRequiredService<ISoftDeletedEntitiesPurger>();
            var deletedCount = await purger.PurgeAsync(cancellationToken);
            CleanupCompleted(_logger, deletedCount);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
#pragma warning disable CA1031
        catch (Exception exception)
        {
            CleanupFailed(_logger, exception);
        }
#pragma warning restore CA1031
    }

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Удаление сущностей завершено. Удалено: {DeletedCount}")]
    private static partial void CleanupCompleted(ILogger logger, int deletedCount);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Error,
        Message = "Удаление сущностей провалилось; в следующий раз повторится")]
    private static partial void CleanupFailed(ILogger logger, Exception exception);
}
