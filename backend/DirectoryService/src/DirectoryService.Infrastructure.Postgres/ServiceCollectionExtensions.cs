using DirectoryService.Core.Departments;
using DirectoryService.Core.Abstractions;
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
		services.AddScoped<ITransactionManager, TransactionManager>();
		services.AddSingleton<IDbConnectionFactory, NpgsqlDbConnectionFactory>();
		return services;
	}
}
