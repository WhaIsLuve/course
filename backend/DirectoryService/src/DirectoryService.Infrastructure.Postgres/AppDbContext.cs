using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Core.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres;

public partial class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IReadDbContext
{
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DepartmentLocation> DepartmentLocations => Set<DepartmentLocation>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<DepartmentPosition> DepartmentPositions => Set<DepartmentPosition>();

    IQueryable<Location> IReadDbContext.Locations => Locations.AsNoTracking();

    IQueryable<Department> IReadDbContext.Departments => Departments.AsNoTracking();

    IQueryable<DepartmentLocation> IReadDbContext.DepartmentLocations => DepartmentLocations.AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}