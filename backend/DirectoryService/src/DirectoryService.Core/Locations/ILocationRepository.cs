using DirectoryService.Domain.Locations;

namespace DirectoryService.Core.Locations;

public interface ILocationRepository
{
	void Add(Location location);

	ValueTask<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

	Task<IReadOnlyList<Location>> GetByIdsAsync(IReadOnlyList<Guid> locationIds,
		CancellationToken cancellationToken = default);

	Task<bool> ExistWithSameNameAsync(string name, CancellationToken cancellationToken = default);

	Task Save(CancellationToken cancellationToken = default);
}