using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Extensions;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Logging;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Features.Locations.Create;

public sealed class CreateLocationHandler(
    TimeProvider timeProvider,
    ILocationRepository locationRepository,
    IValidator<CreateLocationDto> validator,
    ILogger<CreateLocationHandler> logger)
    : ICommandHandler<CreateLocationCommand, Result<Guid, Error>>
{
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly ILocationRepository _locationRepository =
        locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
    private readonly IValidator<CreateLocationDto> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly ILogger<CreateLocationHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result<Guid, Error>> HandleAsync(
        CreateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Dto, cancellationToken);
        if (!validationResult.IsValid)
            return Error.Validation(validationResult.ToErrorMessages());

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
        var result = await _locationRepository.Save(cancellationToken);
        if (result.IsFailure) return result.Error;

        _logger.LocationCreated(id);
        return id;
    }
}
