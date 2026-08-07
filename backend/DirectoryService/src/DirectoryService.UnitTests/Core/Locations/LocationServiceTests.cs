using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;
using DirectoryService.SharedKernel.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ValidationException = FluentValidation.ValidationException;

namespace DirectoryService.UnitTests.Core.Locations;

public class LocationServiceTests
{
	private readonly Mock<IValidator<CreateLocationDto>> _createValidatorMock;
	private readonly Mock<ILocationRepository> _repositoryMock;
	private readonly LocationService _sut;
	private readonly TimeProvider _timeProvider;
	private readonly Mock<IValidator<UpdateLocationDto>> _updateValidatorMock;

	public LocationServiceTests()
	{
		_repositoryMock = new Mock<ILocationRepository>();
		_createValidatorMock = new Mock<IValidator<CreateLocationDto>>();
		_updateValidatorMock = new Mock<IValidator<UpdateLocationDto>>();
		_timeProvider = TimeProvider.System;
		_sut = new LocationService(_timeProvider, _repositoryMock.Object, _createValidatorMock.Object,
			_updateValidatorMock.Object);
	}

	[Fact]
	public async Task CreateAsyncWithValidDataShouldReturnIdAndCallRepository()
	{
		// Arrange
		var dto = new CreateLocationDto("Test Location", new AddressDto("Country", "City", "Street", "Building"));

		_createValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<CreateLocationDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_repositoryMock
			.Setup(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		_repositoryMock
			.Setup(r => r.Add(It.IsAny<Location>()));

		// Act
		var result = await _sut.CreateAsync(dto, CancellationToken.None);

		// Assert
		Assert.NotEqual(Guid.Empty, result);
		_repositoryMock.Verify(r => r.Add(It.IsAny<Location>()), Times.Once);
	}

	[Fact]
	public async Task CreateAsyncWithInvalidDtoShouldReturnValidationError()
	{
		// Arrange
		var dto = new CreateLocationDto("", new AddressDto("", "", "", ""));
		var validationFailures = new List<ValidationFailure>
		{
			new("Name", "Имя обязательное поле")
		};

		_createValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<CreateLocationDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult(validationFailures));

		// Act
		var result = await _sut.CreateAsync(dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(ErrorType.Validation, result.Error.Type);
		_repositoryMock.Verify(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_repositoryMock.Verify(r => r.Add(It.IsAny<Location>()), Times.Never);
	}

	[Fact]
	public async Task CreateAsyncWithExistingNameShouldReturnConflictError()
	{
		// Arrange
		var dto = new CreateLocationDto("Existing Location", new AddressDto("Country", "City", "Street", "Building"));

		_createValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<CreateLocationDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_repositoryMock
			.Setup(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		var result = await _sut.CreateAsync(dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(ErrorType.Conflict, result.Error.Type);
		_repositoryMock.Verify(r => r.Add(It.IsAny<Location>()), Times.Never);
	}

	[Fact]
	public async Task CreateAsyncWithInvalidAddressShouldReturnValidationError()
	{
		// Arrange
		var invalidCountry = new string('a', Address.CountryMaxLength + 1);
		var dto = new CreateLocationDto("Test Location", new AddressDto(invalidCountry, "City", "Street", "Building"));

		_createValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<CreateLocationDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_repositoryMock
			.Setup(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		var result = await _sut.CreateAsync(dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(ErrorType.Validation, result.Error.Type);
		_repositoryMock.Verify(r => r.Add(It.IsAny<Location>()), Times.Never);
	}

	[Fact]
	public async Task CreateAsyncWithInvalidLocationNameShouldReturnValidationError()
	{
		// Arrange
		var invalidName = new string('a', LocationName.MaxLength + 1);
		var dto = new CreateLocationDto(invalidName, new AddressDto("Country", "City", "Street", "Building"));

		_createValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<CreateLocationDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_repositoryMock
			.Setup(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		var result = await _sut.CreateAsync(dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(ErrorType.Validation, result.Error.Type);
		_repositoryMock.Verify(r => r.Add(It.IsAny<Location>()), Times.Never);
	}





	[Fact]
	public async Task UpdateAsyncWithValidDataAndChangedNameShouldUpdateAndCheckUniqueness()
	{
		// Arrange
		var locationId = Guid.NewGuid();
		var existingLocation = CreateMockLocation(locationId);
		var dto = new UpdateLocationDto("New Location Name", new AddressDto("Country", "City", "Street", "Building"));

		_updateValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<UpdateLocationDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_repositoryMock
			.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingLocation);

		_repositoryMock
			.Setup(r => r.ExistWithSameNameAsync("New Location Name", It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		await _sut.UpdateAsync(locationId, dto, CancellationToken.None);

		// Assert
		_repositoryMock.Verify(
			r => r.ExistWithSameNameAsync("New Location Name", It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task UpdateAsyncWithValidDataAndSameNameShouldUpdateWithoutCheckingUniqueness()
	{
		// Arrange
		var locationId = Guid.NewGuid();
		var existingLocation = CreateMockLocation(locationId);
		var dto = new UpdateLocationDto("Location Name", new AddressDto("New Country", "City", "Street", "Building"));

		_updateValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<UpdateLocationDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_repositoryMock
			.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingLocation);

		// Act
		await _sut.UpdateAsync(locationId, dto, CancellationToken.None);

		// Assert
		_repositoryMock.Verify(
			r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task UpdateAsyncWithInvalidDtoShouldReturnValidationError()
	{
		// Arrange
		var locationId = Guid.NewGuid();
		var dto = new UpdateLocationDto("", new AddressDto("", "", "", ""));
		var validationFailures = new List<ValidationFailure>
		{
			new("Name", "Имя обязательное поле")
		};

		_updateValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<UpdateLocationDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult(validationFailures));

		// Act
		var result = await _sut.UpdateAsync(locationId, dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(ErrorType.Validation, result.Error.Type);
		_repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task UpdateAsyncWithNonExistentLocationShouldReturnNotFoundError()
	{
		// Arrange
		var locationId = Guid.NewGuid();
		var dto = new UpdateLocationDto("New Name", new AddressDto("Country", "City", "Street", "Building"));
		var expectedError = Error.NotFound("not found", "not found");

		_updateValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<UpdateLocationDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_repositoryMock
			.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.UpdateAsync(locationId, dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError.Type, result.Error.Type);
	}

	[Fact]
	public async Task UpdateAsyncWithInvalidDomainNameShouldReturnValidationError()
	{
		// Arrange
		var locationId = Guid.NewGuid();
		var invalidName = new string('a', LocationName.MaxLength + 1);
		var existingLocation = CreateMockLocation(locationId);
		var dto = new UpdateLocationDto(invalidName, new AddressDto("Country", "City", "Street", "Building"));

		_updateValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<UpdateLocationDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_repositoryMock
			.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingLocation);

		// Act
		var result = await _sut.UpdateAsync(locationId, dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(ErrorType.Validation, result.Error.Type);
	}

	[Fact]
	public async Task UpdateAsyncWithInvalidDomainAddressShouldReturnValidationError()
	{
		// Arrange
		var locationId = Guid.NewGuid();
		var invalidCountry = new string('a', Address.CountryMaxLength + 1);
		var existingLocation = CreateMockLocation(locationId);
		var dto = new UpdateLocationDto("New Name", new AddressDto(invalidCountry, "City", "Street", "Building"));

		_updateValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<UpdateLocationDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_repositoryMock
			.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingLocation);

		// Act
		var result = await _sut.UpdateAsync(locationId, dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(ErrorType.Validation, result.Error.Type);
	}

	[Fact]
	public async Task UpdateAsyncWithExistingNameShouldReturnConflictError()
	{
		// Arrange
		var locationId = Guid.NewGuid();
		var existingLocation = CreateMockLocation(locationId);
		var dto = new UpdateLocationDto("Existing Name", new AddressDto("Country", "City", "Street", "Building"));

		_updateValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<UpdateLocationDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_repositoryMock
			.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingLocation);

		_repositoryMock
			.Setup(r => r.ExistWithSameNameAsync("Existing Name", It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		var result = await _sut.UpdateAsync(locationId, dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(ErrorType.Conflict, result.Error.Type);
	}

	[Fact]
	public async Task CreateAsyncWhenSaveFailsShouldReturnFailure()
	{
		// Arrange
		var dto = new CreateLocationDto("Location", new AddressDto("Country", "City", null, null));
		var expectedError = Error.Failure("database.save.error", "Save failed");
		_createValidatorMock
			.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());
		_repositoryMock
			.Setup(r => r.ExistWithSameNameAsync(dto.Name, It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);
		_repositoryMock
			.Setup(r => r.Save(It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.CreateAsync(dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError, result.Error);
		_repositoryMock.Verify(r => r.Add(It.IsAny<Location>()), Times.Once);
	}

	[Fact]
	public async Task UpdateAsyncWhenSaveFailsShouldReturnFailure()
	{
		// Arrange
		var locationId = Guid.NewGuid();
		var location = CreateMockLocation(locationId);
		var dto = new UpdateLocationDto("Location Name", new AddressDto("Country", "New City", null, null));
		var expectedError = Error.Failure("database.save.error", "Save failed");
		_updateValidatorMock
			.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());
		_repositoryMock
			.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(location);
		_repositoryMock
			.Setup(r => r.Save(It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.UpdateAsync(locationId, dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError, result.Error);
	}

	private static Location CreateMockLocation(Guid id)
	{
		var name = LocationName.Create("Location Name").Value;
		var address = Address.Create("Country", "City", null, null).Value;
		return Location.Create(id, name, address, DateTime.UtcNow).Value;
	}
}
