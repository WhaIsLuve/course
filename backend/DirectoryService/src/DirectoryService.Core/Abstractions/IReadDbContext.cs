using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Core.Abstractions;

public interface IReadDbContext
{
    IQueryable<Location> Locations { get; }

    IQueryable<Department> Departments { get; }

    IQueryable<Position> Positions { get; }

    IQueryable<DepartmentLocation> DepartmentLocations { get; }

    IQueryable<DepartmentPosition> DepartmentPositions { get; }
}
