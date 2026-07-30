using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Extensions;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;
using DirectoryService.SharedKernel.Exceptions;
using FluentValidation;
using ValidationException = DirectoryService.SharedKernel.Exceptions.ValidationException;

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

	public async Task<Guid> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken)
	{
		var validationResult = await _createLocationDtoValidator.ValidateAsync(dto, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(Error.Validation(validationResult.ToErrorMessages()));

		var existWithSameName = await _locationRepository.ExistWithSameNameAsync(dto.Name, cancellationToken);
		if (existWithSameName)
			throw new ConflictException(Error.Conflict("location.name.exists",
				"Локация с таким наименование уже существует"));
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
		if (location.IsFailure) throw new FailureException(location.Error);

		_locationRepository.Add(location.Value);
		await _locationRepository.Save(cancellationToken);

		return id;
	}

	public async Task UpdateAsync(Guid id, UpdateLocationDto dto, CancellationToken cancellationToken = default)
	{
		var validationResult = await _updateLocationDtoValidator.ValidateAsync(dto, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(Error.Validation(validationResult.ToErrorMessages()));

		var location = await _locationRepository.GetByIdAsync(id, cancellationToken) ??
		               throw new NotFoundException(Error.NotFound("location.not.found", "Локация не найдена"));
		var locationName = LocationName.Create(dto.Name);
		var address = Address.Create(dto.Address.Country,
			dto.Address.City,
			dto.Address.Street,
			dto.Address.Building);
		if (address.IsFailure) throw new ValidationException(address.Error);
		if (locationName.IsFailure) throw new ValidationException(locationName.Error);
		if (!string.Equals(location.Name.Value, dto.Name, StringComparison.OrdinalIgnoreCase))
		{
			var existWithSameName = await _locationRepository.ExistWithSameNameAsync(dto.Name, cancellationToken);
			if (existWithSameName)
				throw new ConflictException(Error.Conflict("location.name.exists",
					"Локация с таким наименование уже существует"));
		}

		var result = location.Update(locationName.Value, address.Value, _timeProvider.GetUtcNow().UtcDateTime);
		if (result.IsFailure) throw new FailureException(result.Error);
		await _locationRepository.Save(cancellationToken);
	}
}