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

namespace DirectoryService.Core.Features.Locations.Update;

public sealed class UpdateLocationHandler(
    TimeProvider timeProvider,
    ILocationRepository locationRepository,
    IValidator<UpdateLocationDto> validator,
    ILogger<UpdateLocationHandler> logger)
    : ICommandHandler<UpdateLocationCommand, UnitResult<Error>>
{
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly ILocationRepository _locationRepository =
        locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
    private readonly IValidator<UpdateLocationDto> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly ILogger<UpdateLocationHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UnitResult<Error>> HandleAsync(
        UpdateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Dto, cancellationToken);
        if (!validationResult.IsValid)
            return Error.Validation(validationResult.ToErrorMessages());

        var location = await _locationRepository.GetByIdAsync(command.Id, cancellationToken);
        if (location.IsFailure) return location.Error;
        var locationName = LocationName.Create(command.Dto.Name);
        if (locationName.IsFailure) return locationName.Error;
        var address = Address.Create(command.Dto.Address.Country,
            command.Dto.Address.City,
            command.Dto.Address.Street,
            command.Dto.Address.Building);
        if (address.IsFailure) return address.Error;
        if (!string.Equals(location.Value.Name.Value, command.Dto.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existWithSameName = await _locationRepository.ExistWithSameNameAsync(command.Dto.Name, cancellationToken);
            if (existWithSameName)
            {
                return Error.Conflict("location.name.exists",
                    "Локация с таким наименованием уже существует");
            }
        }

        var result = location.Value.Update(locationName.Value, address.Value, _timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsFailure) return result.Error;

        result = await _locationRepository.Save(cancellationToken);
        if (result.IsFailure) return result.Error;

        _logger.LocationUpdated(command.Id);
        return UnitResult.Success<Error>();
    }
}
