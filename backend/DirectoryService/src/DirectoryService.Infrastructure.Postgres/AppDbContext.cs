using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Location> Locations => Set<Location>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}