using CSharpFunctionalExtensions;
using DirectoryService.Core.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
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

	public void AddDepartmentPosition(DepartmentPosition departmentPosition)
	{
		_dbContext.DepartmentPositions.Add(departmentPosition);
	}

	public void RemoveDepartmentPosition(DepartmentPosition departmentPosition)
	{
		_dbContext.DepartmentPositions.Remove(departmentPosition);
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

	public Task<bool> ExistDepartmentPosition(Guid departmentId, Guid positionId,
		CancellationToken cancellationToken = default)
	{
		return _dbContext.DepartmentPositions.AnyAsync(
			dp => dp.DepartmentId == departmentId && dp.PositionId == positionId, cancellationToken);
	}

	public async Task<Result<DepartmentPosition, Error>> GetDepartmentPosition(Guid departmentId, Guid positionId,
		CancellationToken cancellationToken = default)
	{
		var departmentPosition = await _dbContext.DepartmentPositions.SingleOrDefaultAsync(
			dp => dp.DepartmentId == departmentId && dp.PositionId == positionId, cancellationToken);

		return departmentPosition.ToResult(Error.NotFound("department.position.not.found",
			"Связи между подразделением и должностью не существует."));
	}

	public async ValueTask<Result<Department, Error>> GetByIdAsync(Guid id,
		CancellationToken cancellationToken = default)
	{
		var department = await _dbContext.Departments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

		return department.ToResult(Error.NotFound("department.not.found", $"Департамент с идентификатором {id} не найден"));
	}

	public Task<bool> HasActiveChildrenAsync(Guid departmentId, CancellationToken cancellationToken = default)
	{
		return _dbContext.Departments.AnyAsync(x => x.ParentId == departmentId, cancellationToken);
	}

}
