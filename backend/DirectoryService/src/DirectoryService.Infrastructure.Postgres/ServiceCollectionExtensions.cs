using DirectoryService.Core.Locations;
using DirectoryService.Infrastructure.Postgres.DataStorage;
using DirectoryService.Infrastructure.Postgres.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
#pragma warning disable S125
		// services.AddScoped<ILocationRepository, DapperLocationsRepository>();
#pragma warning restore S125
		services.AddScoped<ILocationRepository, EfCoreLocationRepository>();
		services.AddSingleton<IDbConnectionFactory, NpgsqlDbConnectionFactory>();
		return services;
	}
}