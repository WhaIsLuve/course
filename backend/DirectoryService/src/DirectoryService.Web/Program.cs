using DirectoryService.Core;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddControllers();
builder.Services.AddCore();
builder.Services.AddInfrastructure();
builder.Services.AddHealthChecks();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
	.UseLoggerFactory(LoggerFactory.Create(c => c.AddConsole())));

var app = builder.Build();

app.MapControllers();

app.MapHealthChecks("/health");

if (!app.Environment.IsProduction())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

await app.RunAsync();