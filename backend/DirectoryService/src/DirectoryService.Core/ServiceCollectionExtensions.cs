using DirectoryService.Core.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Core;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCore(this IServiceCollection services)
	{
		services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
		services.Scan(scan => scan
			.FromAssemblies(typeof(ServiceCollectionExtensions).Assembly)
			.AddClasses(classes => classes.AssignableToAny(
				typeof(ICommandHandler<,>),
				typeof(IQueryHandler<,>)))
			.AsImplementedInterfaces()
			.WithScopedLifetime());
		return services;
	}
}
