namespace DirectoryService.Infrastructure.Postgres.BackgroundCleanup;

public sealed class SoftDeleteCleanupOptions
{
    public TimeSpan RunInterval { get; set; } = TimeSpan.FromDays(1);

    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(30);

    public int BatchSize { get; set; } = 500;
}
