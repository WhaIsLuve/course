using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Logging;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Features.Locations.Create;

public sealed class CreateLocationHandler(
    TimeProvider timeProvider,
    ILocationRepository locationRepository,
    ILogger<CreateLocationHandler> logger)
    : ICommandHandler<CreateLocationCommand, Result<Guid, Error>>
{
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly ILocationRepository _locationRepository =
        locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
    private readonly ILogger<CreateLocationHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result<Guid, Error>> HandleAsync(
        CreateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var existWithSameName = await _locationRepository.ExistWithSameNameAsync(command.Dto.Name, cancellationToken);
        if (existWithSameName)
        {
            return Error.Conflict("location.name.exists",
                "Локация с таким наименованием уже существует");
        }

        var id = Guid.CreateVersion7();
        var locationName = LocationName.Create(command.Dto.Name);
        if (locationName.IsFailure) return locationName.Error;
        var address = Address.Create(command.Dto.Address.Country,
            command.Dto.Address.City,
            command.Dto.Address.Street,
            command.Dto.Address.Building);
        if (address.IsFailure) return address.Error;

        var location = Location.Create(id, locationName.Value, address.Value, _timeProvider.GetUtcNow().UtcDateTime);
        if (location.IsFailure) return location.Error;

        _locationRepository.Add(location.Value);
        _logger.LocationCreated(id);
        return id;
    }
}
