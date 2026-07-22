using DirectoryService.Domain.Locations;

namespace DirectoryService.Core.Locations;

public interface ILocationRepository
{
	Task AddAsync(Location location, CancellationToken cancellationToken = default);

	Task<IReadOnlyList<Location>> GetByIdsAsync(IReadOnlyList<Guid> locationIds,
		CancellationToken cancellationToken = default);

	Task<bool> ExistWithSameNameAsync(string name, CancellationToken cancellationToken = default);
}