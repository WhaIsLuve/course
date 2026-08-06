using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Extensions;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;
using FluentValidation;

namespace DirectoryService.Core.Locations;

public sealed class LocationService(
	TimeProvider timeProvider,
	ILocationRepository locationRepository,
	IValidator<CreateLocationDto> createLocationDtoValidator,
	IValidator<UpdateLocationDto> updateLocationDtoValidator)
	: ILocationService
{
	private readonly IValidator<CreateLocationDto> _createLocationDtoValidator = createLocationDtoValidator ??
		throw new ArgumentNullException(nameof(createLocationDtoValidator));

	private readonly ILocationRepository _locationRepository =
		locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));

	private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

	private readonly IValidator<UpdateLocationDto> _updateLocationDtoValidator = updateLocationDtoValidator ??
		throw new ArgumentNullException(nameof(updateLocationDtoValidator));

	public async Task<Result<Guid, Error>> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken)
	{
		var validationResult = await _createLocationDtoValidator.ValidateAsync(dto, cancellationToken);
		if (!validationResult.IsValid)
			return Error.Validation(validationResult.ToErrorMessages());

		var existWithSameName = await _locationRepository.ExistWithSameNameAsync(dto.Name, cancellationToken);
		if (existWithSameName)
        {
            return Error.Conflict("location.name.exists",
				"Локация с таким наименование уже существует");
        }

        var id = Guid.CreateVersion7();
		var locationName = LocationName.Create(dto.Name);
		if (locationName.IsFailure) return locationName.Error;
		var address = Address.Create(dto.Address.Country,
			dto.Address.City,
			dto.Address.Street,
			dto.Address.Building);
		if (address.IsFailure) return address.Error;

		var location = Location.Create(id, locationName.Value, address.Value, _timeProvider.GetUtcNow()
			.UtcDateTime);
		if (location.IsFailure) return location.Error;

		_locationRepository.Add(location.Value);
		var result = await _locationRepository.Save(cancellationToken);
		if (result.IsFailure) return result.Error;

		return id;
	}

	public async Task<UnitResult<Error>> UpdateAsync(Guid id, UpdateLocationDto dto,
		CancellationToken cancellationToken = default)
	{
		var validationResult = await _updateLocationDtoValidator.ValidateAsync(dto, cancellationToken);
		if (!validationResult.IsValid)
			return Error.Validation(validationResult.ToErrorMessages());

		var location = await _locationRepository.GetByIdAsync(id, cancellationToken);
		if (location.IsFailure) return location.Error;
		var locationName = LocationName.Create(dto.Name);
		if (locationName.IsFailure) return locationName.Error;
		var address = Address.Create(dto.Address.Country,
			dto.Address.City,
			dto.Address.Street,
			dto.Address.Building);
		if (address.IsFailure) return address.Error;
		if (!string.Equals(location.Value.Name.Value, dto.Name, StringComparison.OrdinalIgnoreCase))
		{
			var existWithSameName = await _locationRepository.ExistWithSameNameAsync(dto.Name, cancellationToken);
			if (existWithSameName)
			{
				return Error.Conflict("location.name.exists",
					"Локация с таким наименование уже существует");
			}
		}

		var result = location.Value.Update(locationName.Value, address.Value, _timeProvider.GetUtcNow().UtcDateTime);
		if (result.IsFailure) return result.Error;
		return await _locationRepository.Save(cancellationToken);
	}
}