using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Locations;

public interface ILocationRepository
{
	void Add(Location location);
	void Remove(Location location);

	ValueTask<Result<Location, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

	Task<Result<IReadOnlyList<Location>, Error>> GetByIdsAsync(IReadOnlyList<Guid> locationIds,
		CancellationToken cancellationToken = default);

	Task<bool> ExistWithSameNameAsync(string name, CancellationToken cancellationToken = default);

	Task<bool> HasDepartmentLinksAsync(Guid locationId, CancellationToken cancellationToken = default);

}
