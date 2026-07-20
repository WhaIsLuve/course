using DirectoryService.Contracts.Locations;

namespace DirectoryService.Core.Locations;

public interface ILocationService
{
    Task<Guid> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken);
}