using DirectoryService.Infrastructure.Postgres.BackgroundCleanup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectoryService.IntegrationTests.Infrastructure;

public sealed class SoftDeleteCleanupRunnerTests
{
    [Fact]
    public async Task FailedRunIsSwallowedAndNextRunIsAttempted()
    {
        var purger = new SequencePurger();
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddScoped<ISoftDeletedEntitiesPurger>(_ => purger)
            .BuildServiceProvider();
        var runner = new SoftDeleteCleanupRunner(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILogger<SoftDeleteCleanupRunner>>());

        await runner.RunOnceAsync();
        await runner.RunOnceAsync();

        Assert.Equal(2, purger.Attempts);
    }

    private sealed class SequencePurger : ISoftDeletedEntitiesPurger
    {
        public int Attempts { get; private set; }

        public Task<int> PurgeAsync(CancellationToken cancellationToken = default)
        {
            Attempts++;
            return Attempts == 1
                ? Task.FromException<int>(new InvalidOperationException("Expected test failure"))
                : Task.FromResult(0);
        }
    }
}
