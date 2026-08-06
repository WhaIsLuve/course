using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Locations;

public interface ILocationService
{
    Task<Result<Guid, Error>> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken);

    Task<UnitResult<Error>> UpdateAsync(Guid id, UpdateLocationDto dto, CancellationToken cancellationToken = default);
}