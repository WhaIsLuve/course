using DirectoryService.Infrastructure.Postgres.BackgroundCleanup;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Web.BackgroundCleanup;

internal sealed partial class SoftDeleteCleanupBackgroundService(
    SoftDeleteCleanupRunner runner,
    IOptions<SoftDeleteCleanupOptions> options,
    ILogger<SoftDeleteCleanupBackgroundService> logger) : BackgroundService
{
    private readonly SoftDeleteCleanupRunner _runner = runner ?? throw new ArgumentNullException(nameof(runner));
    private readonly IOptions<SoftDeleteCleanupOptions> _options =
        options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<SoftDeleteCleanupBackgroundService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ServiceStarted(_logger);

        while (!stoppingToken.IsCancellationRequested)
        {
            await _runner.RunOnceAsync(stoppingToken);

            try
            {
                await Task.Delay(_options.Value.RunInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        ServiceStopped(_logger);
    }

    [LoggerMessage(
        EventId = 2010,
        Level = LogLevel.Information,
        Message = "Очистка удаленных сущностей началась")]
    private static partial void ServiceStarted(ILogger logger);

    [LoggerMessage(
        EventId = 2011,
        Level = LogLevel.Information,
        Message = "Очистка удаленных сущностей остановлена")]
    private static partial void ServiceStopped(ILogger logger);
}
