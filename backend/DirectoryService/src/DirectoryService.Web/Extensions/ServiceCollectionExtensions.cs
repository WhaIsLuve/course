using Serilog;

namespace DirectoryService.Web.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSerilogLogger(this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddSerilog((_, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration));
    }
}
