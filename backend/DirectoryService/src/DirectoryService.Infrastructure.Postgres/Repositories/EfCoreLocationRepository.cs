using DirectoryService.Core.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

internal sealed class EfCoreLocationRepository(AppDbContext dbContext) : ILocationRepository
{
	private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

	public Task AddAsync(Location location, CancellationToken cancellationToken = default)
	{
		_dbContext.Locations.Add(location);
		return _dbContext.SaveChangesAsync(cancellationToken);
	}

	public Task<bool> ExistWithSameNameAsync(string name, CancellationToken cancellationToken = default)
	{
#pragma warning disable CA1862, RCS1155, CA1304, MA0011, CA1304, CA1311
		return _dbContext.Locations.AnyAsync(l => l.Name.Value.ToUpper() == name.ToUpper(), cancellationToken);
#pragma warning restore CA1311, CA1304, MA0011, CA1304, RCS1155, CA1862
	}
}