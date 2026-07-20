using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using FluentValidation;

namespace DirectoryService.Core.Locations;

internal sealed class LocationService(
    TimeProvider timeProvider,
    ILocationRepository locationRepository,
    IValidator<CreateLocationDto> createLocationDtoValidator)
    : ILocationService
{
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

    private readonly ILocationRepository _locationRepository =
        locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));

    private readonly IValidator<CreateLocationDto> _createLocationDtoValidator = createLocationDtoValidator ??
        throw new ArgumentNullException(nameof(createLocationDtoValidator));

    public async Task<Guid> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _createLocationDtoValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var existWithSameName = await _locationRepository.ExistWithSameNameAsync(dto.Name, cancellationToken);
        if (existWithSameName) throw new InvalidOperationException("Локация с таким наименование уже существует");
        var id = Guid.CreateVersion7();
        var locationName = LocationName.Create(dto.Name);
        var address = Address.Create(dto.Address.Country,
            dto.Address.City,
            dto.Address.Street,
            dto.Address.Building);
        if (address.IsFailure) throw new ValidationException(address.Error);
        if (locationName.IsFailure) throw new ValidationException(locationName.Error);

        var location = Location.Create(id, locationName.Value, address.Value, _timeProvider.GetUtcNow()
            .UtcDateTime);
        if (location.IsFailure) throw new InvalidOperationException(location.Error);

        await _locationRepository.AddAsync(location.Value, cancellationToken);

        return id;
    }
}