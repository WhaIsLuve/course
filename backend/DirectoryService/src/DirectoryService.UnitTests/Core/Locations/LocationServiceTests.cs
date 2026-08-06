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

/// <summary>
///     Содержит юнит-тесты для класса <see cref="LocationService" />.
/// </summary>
public class LocationServiceTests
{
	private readonly Mock<IValidator<CreateLocationDto>> _createValidatorMock;
	private readonly Mock<ILocationRepository> _repositoryMock;
	private readonly LocationService _sut;
	private readonly TimeProvider _timeProvider;
	private readonly Mock<IValidator<UpdateLocationDto>> _updateValidatorMock;

	/// <summary>
	///     Инициализирует новый экземпляр класса <see cref="LocationServiceTests" />.
	/// </summary>
	public LocationServiceTests()
	{
		_repositoryMock = new Mock<ILocationRepository>();
		_createValidatorMock = new Mock<IValidator<CreateLocationDto>>();
		_updateValidatorMock = new Mock<IValidator<UpdateLocationDto>>();
		_timeProvider = TimeProvider.System;
		_sut = new LocationService(_timeProvider, _repositoryMock.Object, _createValidatorMock.Object,
			_updateValidatorMock.Object);
	}

	/// <summary>
	///     Проверяет, что при валидных данных метод успешно создает локацию и вызывает репозиторий.
	/// </summary>
	/// <returns>Задача выполнения теста.</returns>
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

	/// <summary>
	///     Проверяет, что при невалидных данных DTO метод выбрасывает исключение валидации.
	/// </summary>
	/// <returns>Задача выполнения теста.</returns>
	[Fact]
	public async Task CreateAsyncWithInvalidDtoShouldThrowValidationError()
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

	/// <summary>
	///     Проверяет, что при попытке создать локацию с уже существующим именем метод выбрасывает исключение.
	/// </summary>
	/// <returns>Задача выполнения теста.</returns>
	[Fact]
	public async Task CreateAsyncWithExistingNameShouldThrowConflictError()
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

	/// <summary>
	///     Проверяет, что при невалидном адресе (который прошел валидатор DTO) метод выбрасывает исключение валидации.
	/// </summary>
	/// <returns>Задача выполнения теста.</returns>
	[Fact]
	public async Task CreateAsyncWithInvalidAddressShouldThrowValidationError()
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

	/// <summary>
	///     Проверяет, что при невалидном имени локации (которое прошло валидатор DTO) метод выбрасывает исключение валидации.
	/// </summary>
	/// <returns>Задача выполнения теста.</returns>
	[Fact]
	public async Task CreateAsyncWithInvalidLocationNameShouldThrowValidationError()
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

	/// <summary>
	///     Проверяет, что конструктор выбрасывает исключение при передаче null в качестве TimeProvider.
	/// </summary>
	[Fact]
	public void ConstructorWithNullTimeProviderShouldThrowArgumentNullException()
	{
		// Act & Assert
		var exception = Assert.Throws<ArgumentNullException>(() =>
			new LocationService(null!, _repositoryMock.Object, _createValidatorMock.Object,
				_updateValidatorMock.Object));

		Assert.Equal("timeProvider", exception.ParamName);
	}

	/// <summary>
	///     Проверяет, что конструктор выбрасывает исключение при передаче null в качестве репозитория.
	/// </summary>
	[Fact]
	public void ConstructorWithNullRepositoryShouldThrowArgumentNullException()
	{
		// Act & Assert
		var exception = Assert.Throws<ArgumentNullException>(() =>
			new LocationService(_timeProvider, null!, _createValidatorMock.Object, _updateValidatorMock.Object));

		Assert.Equal("locationRepository", exception.ParamName);
	}

	/// <summary>
	///     Проверяет, что конструктор выбрасывает исключение при передаче null в качестве валидатора.
	/// </summary>
	[Fact]
	public void ConstructorWithNullValidatorShouldThrowArgumentNullException()
	{
		// Act & Assert
		var exception = Assert.Throws<ArgumentNullException>(() =>
			new LocationService(_timeProvider, _repositoryMock.Object, null!, _updateValidatorMock.Object));

		Assert.Equal("createLocationDtoValidator", exception.ParamName);
	}

	/// <summary>
	///     Проверяет, что конструктор выбрасывает исключение при передаче null в качестве валидатора.
	/// </summary>
	[Fact]
	public void ConstructorWithNullUpdateValidatorShouldThrowArgumentNullException()
	{
		// Act & Assert
		var exception = Assert.Throws<ArgumentNullException>(() =>
			new LocationService(_timeProvider, _repositoryMock.Object, _createValidatorMock.Object, null!));

		Assert.Equal("updateLocationDtoValidator", exception.ParamName);
	}

	/// <summary>
	///     Проверяет, что при валидных данных и изменении имени метод успешно обновляет локацию и проверяет уникальность
	///     имени.
	/// </summary>
	/// <returns>Задача выполнения теста.</returns>
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

	/// <summary>
	///     Проверяет, что при валидных данных без изменения имени метод обновляет локацию без проверки уникальности.
	/// </summary>
	/// <returns>Задача выполнения теста.</returns>
	[Fact]
	public async Task UpdateAsyncWithValidDataAndSameNameShouldUpdateWithoutCheckingUniqueness()
	{
		// Arrange
		var locationId = Guid.NewGuid();
		var existingLocation = CreateMockLocation(locationId);
		// Имя в DTO совпадает с именем в мок-объекте ("Location Name")
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

	/// <summary>
	///     Проверяет, что при невалидных данных DTO метод выбрасывает исключение валидации.
	/// </summary>
	/// <returns>Задача выполнения теста.</returns>
	[Fact]
	public async Task UpdateAsyncWithInvalidDtoShouldThrowValidationError()
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

	/// <summary>
	///     Проверяет, что при несуществующей локации метод выбрасывает NotFoundError.
	/// </summary>
	/// <returns>Задача выполнения теста.</returns>
	[Fact]
	public async Task UpdateAsyncWithNonExistentLocationShouldThrowNotFoundError()
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

	/// <summary>
	///     Проверяет, что при невалидном имени на уровне домена метод выбрасывает ValidationError.
	/// </summary>
	/// <returns>Задача выполнения теста.</returns>
	[Fact]
	public async Task UpdateAsyncWithInvalidDomainNameShouldThrowValidationError()
	{
		// Arrange
		var locationId = Guid.NewGuid();
		var invalidName = new string('a', LocationName.MaxLength + 1);
		var existingLocation = CreateMockLocation(locationId);
		var dto = new UpdateLocationDto(invalidName, new AddressDto("Country", "City", "Street", "Building"));

		// Мокаем валидатор как успешный, чтобы проверить именно доменную валидацию
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

	/// <summary>
	///     Проверяет, что при невалидном адресе на уровне домена метод выбрасывает ValidationError.
	/// </summary>
	/// <returns>Задача выполнения теста.</returns>
	[Fact]
	public async Task UpdateAsyncWithInvalidDomainAddressShouldThrowValidationError()
	{
		// Arrange
		var locationId = Guid.NewGuid();
		var invalidCountry = new string('a', Address.CountryMaxLength + 1);
		var existingLocation = CreateMockLocation(locationId);
		var dto = new UpdateLocationDto("New Name", new AddressDto(invalidCountry, "City", "Street", "Building"));

		// Мокаем валидатор как успешный, чтобы проверить именно доменную валидацию
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

	/// <summary>
	///     Проверяет, что при попытке обновить локацию на уже существующее имя метод выбрасывает ConflictError.
	/// </summary>
	/// <returns>Задача выполнения теста.</returns>
	[Fact]
	public async Task UpdateAsyncWithExistingNameShouldThrowConflictError()
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

	/// <summary>
	///     Создает валидный мок-объект <see cref="Location" /> для использования в тестах.
	/// </summary>
	/// <param name="id">Идентификатор локации.</param>
	/// <returns>Валидный объект <see cref="Location" />.</returns>
	private static Location CreateMockLocation(Guid id)
	{
		var name = LocationName.Create("Location Name").Value;
		var address = Address.Create("Country", "City", null, null).Value;
		return Location.Create(id, name, address, DateTime.UtcNow).Value;
	}
}