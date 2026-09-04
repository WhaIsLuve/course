using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Core.Abstractions;

public interface IReadDbContext
{
    IQueryable<Location> Locations { get; }

    IQueryable<Department> Departments { get; }

    IQueryable<DepartmentLocation> DepartmentLocations { get; }
}