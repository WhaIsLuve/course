using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres;

public partial class AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger)
	: DbContext(options)
{
	public DbSet<Location> Locations => Set<Location>();
	public DbSet<Department> Departments => Set<Department>();
	public DbSet<DepartmentLocation> DepartmentLocations => Set<DepartmentLocation>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}

	public async Task<UnitResult<Error>> SaveAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await SaveChangesAsync(cancellationToken);
			return UnitResult.Success<Error>();
		}
		catch (Exception ex) when (ex is DbUpdateConcurrencyException or DbUpdateException)
		{
			LogErrorOfSave(ex);
			return Error.Failure("database.save.error", "Произошла ошибка при сохранении данных");
		}
	}

	[LoggerMessage(LogLevel.Error, "Произошла ошибка при сохранении данных")]
	partial void LogErrorOfSave(Exception exception);
}