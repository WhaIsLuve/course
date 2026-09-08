using DirectoryService.Infrastructure.Postgres;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace DirectoryService.IntegrationTests.Infrastructure;

#pragma warning disable CA1515
public sealed class DirectoryServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("directory_service_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner? _respawner;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _ = CreateClient();

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        await dbContext.Database.OpenConnectionAsync();
        _respawner = await Respawner.CreateAsync(
            dbContext.Database.GetDbConnection(),
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = [new Table("public", "__EFMigrationsHistory")]
            });
    }

    public async Task ResetDatabaseAsync()
    {
        var respawner = _respawner ?? throw new InvalidOperationException("Respawn не инициализирован.");

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.OpenConnectionAsync();
        await respawner.ResetAsync(dbContext.Database.GetDbConnection());
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString()
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
#pragma warning restore CA1515
