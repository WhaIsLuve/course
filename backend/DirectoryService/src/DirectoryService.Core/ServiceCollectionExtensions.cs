using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Decorators;
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
			.AddClasses(classes => classes
				.InNamespaces("DirectoryService.Core.Features")
				.AssignableToAny(
					typeof(ICommandHandler<,>),
					typeof(IQueryHandler<,>)))
			.AsImplementedInterfaces()
			.WithScopedLifetime());

		services.Decorate(typeof(ICommandHandler<,>), typeof(CommandTransactionHandlerDecorator<,>));
		services.Decorate(typeof(ICommandHandler<,>), typeof(CommandValidationHandlerDecorator<,>));
		services.Decorate(typeof(ICommandHandler<,>), typeof(CommandLoggingHandlerDecorator<,>));
		return services;
	}
}
