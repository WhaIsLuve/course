using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres.BackgroundCleanup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSoftDeleteCleanup(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SoftDeleteCleanupOptions>()
            .Bind(configuration.GetSection("SoftDeleteCleanup"))
            .Validate(options => options.RunInterval > TimeSpan.Zero,
                "SoftDeleteCleanup:RunInterval must be positive")
            .Validate(options => options.RetentionPeriod > TimeSpan.Zero,
                "SoftDeleteCleanup:RetentionPeriod must be positive")
            .Validate(options => options.BatchSize > 0,
                "SoftDeleteCleanup:BatchSize must be positive")
            .ValidateOnStart();

        services.AddScoped<ISoftDeletedEntitiesPurger, SoftDeletedEntitiesPurger>();
        services.AddSingleton<SoftDeleteCleanupRunner>();
        return services;
    }
}
