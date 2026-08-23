using CSharpFunctionalExtensions;
using DirectoryService.Core.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

internal sealed class DepartmentRepository(AppDbContext dbContext)
	: IDepartmentRepository
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

	public async Task<Result<DepartmentLocation, Error>> GetDepartmentLocation(Guid departmentId, Guid locationId,
		CancellationToken cancellationToken = default)
	{
		var departmentLocation = await _dbContext.DepartmentLocations.SingleOrDefaultAsync(
			dl => dl.LocationId == locationId && dl.DepartmentId == departmentId, cancellationToken);

		return departmentLocation.ToResult(Error.NotFound("department.location.not.found",
			"Связи между локацией и департаментов не существует."));
	}

	public Task<bool> ExistDepartmentLocation(Guid departmentId, Guid locationId,
		CancellationToken cancellationToken = default)
	{
		return _dbContext.DepartmentLocations.AnyAsync(
			dl => dl.LocationId == locationId && dl.DepartmentId == departmentId, cancellationToken);
	}

	public async ValueTask<Result<Department, Error>> GetByIdAsync(Guid id,
		CancellationToken cancellationToken = default)
	{
		var department = await _dbContext.Departments.FindAsync([id], cancellationToken);

		return department.ToResult(Error.NotFound("department.not.found", $"Департамент с идентификатором {id} не найден"));
	}

}
