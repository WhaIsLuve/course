using DirectoryService.Core.Departments;
using DirectoryService.Core.Locations;
using DirectoryService.Infrastructure.Postgres.DataStorage;
using DirectoryService.Infrastructure.Postgres.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
		services.AddScoped<ILocationRepository, LocationRepository>();
		services.AddScoped<IDepartmentRepository, DepartmentRepository>();
		services.AddSingleton<IDbConnectionFactory, NpgsqlDbConnectionFactory>();
		return services;
	}
}