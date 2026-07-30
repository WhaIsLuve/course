using DirectoryService.Core.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

internal sealed class DepartmentRepository(AppDbContext dbContext) : IDepartmentRepository
{
	private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

	public void AddDepartment(Department department)
	{
		_dbContext.Departments.Add(department);
	}

	public void AddDepartmentLocations(IReadOnlyList<DepartmentLocation> departmentLocations)
	{
		_dbContext.DepartmentLocations.AddRange(departmentLocations);
	}

	public void RemoveDepartmentLocation(DepartmentLocation departmentLocation)
	{
		_dbContext.DepartmentLocations.Remove(departmentLocation);
	}

	public Task<DepartmentLocation?> GetDepartmentLocation(Guid departmentId, Guid locationId,
		CancellationToken cancellationToken = default)
	{
		return _dbContext.DepartmentLocations.SingleOrDefaultAsync(
			dl => dl.LocationId == locationId && dl.DepartmentId == departmentId, cancellationToken);
	}

	public Task<bool> ExistDepartmentLocation(Guid departmentId, Guid locationId,
		CancellationToken cancellationToken = default)
	{
		return _dbContext.DepartmentLocations.AnyAsync(
			dl => dl.LocationId == locationId && dl.DepartmentId == departmentId, cancellationToken);
	}

	public ValueTask<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return _dbContext.Departments.FindAsync([id], cancellationToken);
	}

	public Task Save(CancellationToken cancellationToken = default)
	{
		return _dbContext.SaveChangesAsync(cancellationToken);
	}
}