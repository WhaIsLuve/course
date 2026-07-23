using DirectoryService.Core.Departments;
using DirectoryService.Core.Locations;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Core;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCore(this IServiceCollection services)
	{
		services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
		services.AddScoped<ILocationService, LocationService>();
		services.AddScoped<IDepartmentService, DepartmentService>();
		return services;
	}
}