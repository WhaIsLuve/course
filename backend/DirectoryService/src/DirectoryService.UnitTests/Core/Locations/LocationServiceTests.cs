using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace DirectoryService.UnitTests.Core.Locations;

/// <summary>
///     Содержит юнит-тесты для класса <see cref="LocationService" />.
/// </summary>
public class LocationServiceTests
{
    private readonly Mock<ILocationRepository> _repositoryMock;
    private readonly LocationService _sut;
    private readonly TimeProvider _timeProvider;
    private readonly Mock<IValidator<CreateLocationDto>> _validatorMock;

    /// <summary>
    ///     Инициализирует новый экземпляр класса <see cref="LocationServiceTests" />.
    /// </summary>
    public LocationServiceTests()
    {
        _repositoryMock = new Mock<ILocationRepository>();
        _validatorMock = new Mock<IValidator<CreateLocationDto>>();
        _timeProvider = TimeProvider.System;
        _sut = new LocationService(_timeProvider, _repositoryMock.Object, _validatorMock.Object);
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

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateLocationDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock
            .Setup(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     Проверяет, что при невалидных данных DTO метод выбрасывает исключение валидации.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task CreateAsyncWithInvalidDtoShouldThrowValidationException()
    {
        // Arrange
        var dto = new CreateLocationDto("", new AddressDto("", "", "", ""));
        var validationFailures = new List<ValidationFailure>
        {
            new("Name", "Имя обязательное поле")
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateLocationDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(dto, CancellationToken.None));

        Assert.NotEmpty(exception.Errors);
        _repositoryMock.Verify(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    ///     Проверяет, что при попытке создать локацию с уже существующим именем метод выбрасывает исключение.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task CreateAsyncWithExistingNameShouldThrowInvalidOperationException()
    {
        // Arrange
        var dto = new CreateLocationDto("Existing Location", new AddressDto("Country", "City", "Street", "Building"));

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateLocationDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock
            .Setup(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(dto, CancellationToken.None));

        Assert.Equal("Локация с таким наименование уже существует", exception.Message);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    ///     Проверяет, что при невалидном адресе (который прошел валидатор DTO) метод выбрасывает исключение валидации.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task CreateAsyncWithInvalidAddressShouldThrowValidationException()
    {
        // Arrange
        var invalidCountry = new string('a', Address.CountryMaxLength + 1);
        var dto = new CreateLocationDto("Test Location", new AddressDto(invalidCountry, "City", "Street", "Building"));

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateLocationDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock
            .Setup(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(dto, CancellationToken.None));

        Assert.Contains("Country cannot exceed", exception.Message, StringComparison.OrdinalIgnoreCase);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    ///     Проверяет, что при невалидном имени локации (которое прошло валидатор DTO) метод выбрасывает исключение валидации.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task CreateAsyncWithInvalidLocationNameShouldThrowValidationException()
    {
        // Arrange
        var invalidName = new string('a', LocationName.MaxLength + 1);
        var dto = new CreateLocationDto(invalidName, new AddressDto("Country", "City", "Street", "Building"));

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateLocationDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock
            .Setup(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(dto, CancellationToken.None));

        Assert.Contains("Name cannot exceed", exception.Message, StringComparison.OrdinalIgnoreCase);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    ///     Проверяет, что конструктор выбрасывает исключение при передаче null в качестве TimeProvider.
    /// </summary>
    [Fact]
    public void ConstructorWithNullTimeProviderShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new LocationService(null!, _repositoryMock.Object, _validatorMock.Object));

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
            new LocationService(_timeProvider, null!, _validatorMock.Object));

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
            new LocationService(_timeProvider, _repositoryMock.Object, null!));

        Assert.Equal("createLocationDtoValidator", exception.ParamName);
    }
}