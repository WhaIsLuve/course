using System.Globalization;
using DirectoryService.Core;
using DirectoryService.Infrastructure.Postgres;
using DirectoryService.Infrastructure.Postgres.BackgroundCleanup;
using DirectoryService.Web.Extensions;
using DirectoryService.Web.BackgroundCleanup;
using DirectoryService.Web.Middlewares;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Context;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
	.CreateBootstrapLogger();

Log.Information("DirectoryService запускается");
try
{
	var builder = WebApplication.CreateBuilder(args);

	builder.Services.AddSerilogLogger(builder.Configuration);
	builder.Services.AddOpenApi();
	builder.Services.AddSingleton(TimeProvider.System);
	builder.Services.AddControllers();
	builder.Services.AddCore();
	builder.Services.AddInfrastructure();
	builder.Services.AddSoftDeleteCleanup(builder.Configuration);
	builder.Services.AddHostedService<SoftDeleteCleanupBackgroundService>();
	builder.Services.AddHealthChecks();
	builder.Services.AddDbContext<AppDbContext>(options => options
		.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

	var app = builder.Build();

	app.UseSerilogRequestLogging(options =>
	{
		options.MessageTemplate = "HTTP-запрос {RequestMethod} {RequestPath} завершён с кодом {StatusCode} за {Elapsed:0.0000} мс";
		options.GetLevel = (httpContext, _, exception) =>
		{
			if (exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
			{
				return LogEventLevel.Error;
			}

			return httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest
				? LogEventLevel.Warning
				: LogEventLevel.Information;
		};
		options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
		{
			diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
		};
	});

	app.Use(async (context, next) =>
	{
		using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
		{
			await next(context);
		}
	});

	app.UseMiddleware<ExceptionMiddleware>();

	app.MapControllers();
	app.MapHealthChecks("/health");

	if (!app.Environment.IsProduction())
	{
		app.MapOpenApi();
		app.MapScalarApiReference();
	}

	Log.Logger.Information("Сервис запущен");
	await app.RunAsync();
	Log.Logger.Information("Сервис остановлен");
}
#pragma warning disable CA1031
catch (Exception ex)
#pragma warning restore CA1031
{
	Log.Error(ex, "Что-то пошло не так при запуске приложения.");
}
finally
{
	await Log.CloseAndFlushAsync();
}
