using DirectoryService.Core.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;

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

	public ValueTask<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return _dbContext.Departments.FindAsync([id], cancellationToken);
	}

	public Task Save(CancellationToken cancellationToken = default)
	{
		return _dbContext.SaveChangesAsync(cancellationToken);
	}
}