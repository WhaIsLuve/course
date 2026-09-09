using CSharpFunctionalExtensions;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

internal sealed class LocationRepository(AppDbContext dbContext) : ILocationRepository
{
	private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

	public void Add(Location location)
	{
		_dbContext.Locations.Add(location);
	}

	public async ValueTask<Result<Location, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var location = await _dbContext.Locations.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
		return location.ToResult(Error.NotFound("location.not.found", "Не найдена локация"));
	}

	public async Task<Result<IReadOnlyList<Location>, Error>> GetByIdsAsync(IReadOnlyList<Guid> locationIds,
		CancellationToken cancellationToken = default)
	{
		var locations = await _dbContext.Locations.Where(x => locationIds.Contains(x.Id)).ToListAsync(cancellationToken);
		var missedLocations = locationIds.Except(locations.Select(l => l.Id))
			.ToList();
		if (missedLocations.Count != 0)
		{
			return Error.NotFound("location.missing",
				"Переданы не существующие локации.");
		}

		return locations;
	}

	public Task<bool> ExistWithSameNameAsync(string name, CancellationToken cancellationToken = default)
	{
#pragma warning disable CA1862, RCS1155, CA1304, MA0011, CA1304, CA1311
		return _dbContext.Locations.AnyAsync(l => l.Name.Value.ToUpper() == name.ToUpper(), cancellationToken);
#pragma warning restore CA1311, CA1304, MA0011, CA1304, RCS1155, CA1862
	}

	public Task<bool> HasDepartmentLinksAsync(Guid locationId, CancellationToken cancellationToken = default)
	{
		return _dbContext.DepartmentLocations.AnyAsync(x => x.LocationId == locationId, cancellationToken);
	}

}
