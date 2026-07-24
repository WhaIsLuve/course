using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace DirectoryService.UnitTests.Core.Departments;

/// <summary>
///     Содержит юнит-тесты для класса <see cref="DepartmentService" />.
/// </summary>
public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _departmentRepositoryMock;
    private readonly Mock<ILocationRepository> _locationRepositoryMock;
    private readonly DepartmentService _sut;
    private readonly TimeProvider _timeProvider;
    private readonly Mock<IValidator<CreateDepartmentDto>> _validatorMock;

    /// <summary>
    ///     Инициализирует новый экземпляр класса <see cref="DepartmentServiceTests" />.
    /// </summary>
    public DepartmentServiceTests()
    {
        _validatorMock = new Mock<IValidator<CreateDepartmentDto>>();
        _departmentRepositoryMock = new Mock<IDepartmentRepository>();
        _locationRepositoryMock = new Mock<ILocationRepository>();
        _timeProvider = TimeProvider.System;
        _sut = new DepartmentService(
            _validatorMock.Object,
            _departmentRepositoryMock.Object,
            _timeProvider,
            _locationRepositoryMock.Object);
    }

    /// <summary>
    ///     Проверяет, что при валидных данных корневого подразделения без локаций метод успешно создает подразделение и
    ///     вызывает репозиторий.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task CreateAsyncWithValidRootDepartmentAndNoLocationsShouldReturnIdAndCallRepository()
    {
        // Arrange
        var dto = new CreateDepartmentDto("Department Name", "department-slug", null, new List<Guid>());

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateDepartmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _sut.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _departmentRepositoryMock.Verify(r => r.AddDepartment(It.IsAny<Department>()), Times.Once);
        _departmentRepositoryMock.Verify(r => r.AddDepartmentLocations(It.IsAny<IReadOnlyList<DepartmentLocation>>()),
            Times.Never);
        _departmentRepositoryMock.Verify(r => r.Save(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     Проверяет, что при валидных данных дочернего подразделения с локациями метод успешно создает подразделение, связи и
    ///     вызывает репозиторий.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task CreateAsyncWithValidChildDepartmentAndLocationsShouldReturnIdAndCallRepository()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var dto = new CreateDepartmentDto("Department Name", "department-slug", parentId,
            new List<Guid> { locationId });
        var parentDepartment = CreateMockDepartment(parentId);
        var location = CreateMockLocation(locationId);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateDepartmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _departmentRepositoryMock
            .Setup(r => r.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentDepartment);

        _locationRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Location> { location });

        // Act
        var result = await _sut.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _departmentRepositoryMock.Verify(r => r.AddDepartment(It.IsAny<Department>()), Times.Once);
        _departmentRepositoryMock.Verify(
            r => r.AddDepartmentLocations(It.Is<IReadOnlyList<DepartmentLocation>>(list => list.Count == 1)),
            Times.Once);
        _departmentRepositoryMock.Verify(r => r.Save(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    ///     Проверяет, что при невалидных данных DTO метод выбрасывает исключение валидации и не обращается к репозиторию.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task CreateAsyncWithInvalidDtoShouldThrowValidationException()
    {
        // Arrange
        var dto = new CreateDepartmentDto("", "", Guid.Empty, new List<Guid> { Guid.Empty });
        var validationFailures = new List<ValidationFailure>
        {
            new("Name", "Наименование не может быть пустым")
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateDepartmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(dto, CancellationToken.None));

        _departmentRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _departmentRepositoryMock.Verify(r => r.AddDepartment(It.IsAny<Department>()), Times.Never);
    }

    /// <summary>
    ///     Проверяет, что при указании несуществующего parentId метод выбрасывает InvalidOperationException.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task CreateAsyncWithNonExistentParentShouldThrowInvalidOperationException()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var dto = new CreateDepartmentDto("Name", "slug", parentId, new List<Guid>());

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateDepartmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _departmentRepositoryMock
            .Setup(r => r.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(dto, CancellationToken.None));
        Assert.Equal("Не найден родитель", exception.Message);

        _departmentRepositoryMock.Verify(r => r.AddDepartment(It.IsAny<Department>()), Times.Never);
    }

    /// <summary>
    ///     Проверяет, что при указании несуществующих locationIds метод выбрасывает InvalidOperationException.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task CreateAsyncWithNonExistentLocationsShouldThrowInvalidOperationException()
    {
        // Arrange
        var locationId1 = Guid.NewGuid();
        var locationId2 = Guid.NewGuid();
        var dto = new CreateDepartmentDto("Name", "slug", null, new List<Guid> { locationId1, locationId2 });
        var existingLocation = CreateMockLocation(locationId1);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateDepartmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _locationRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Location> { existingLocation });

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(dto, CancellationToken.None));
        Assert.Equal("Переданы не существующие локации.", exception.Message);

        _departmentRepositoryMock.Verify(r => r.AddDepartment(It.IsAny<Department>()), Times.Never);
    }

    /// <summary>
    ///     Проверяет, что конструктор выбрасывает исключение при передаче null в качестве валидатора.
    /// </summary>
    [Fact]
    public void ConstructorWithNullValidatorShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DepartmentService(null!, _departmentRepositoryMock.Object, _timeProvider,
                _locationRepositoryMock.Object));

        Assert.Equal("validator", exception.ParamName);
    }

    /// <summary>
    ///     Проверяет, что конструктор выбрасывает исключение при передаче null в качестве репозитория подразделений.
    /// </summary>
    [Fact]
    public void ConstructorWithNullRepositoryShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DepartmentService(_validatorMock.Object, null!, _timeProvider, _locationRepositoryMock.Object));

        Assert.Equal("repository", exception.ParamName);
    }

    /// <summary>
    ///     Проверяет, что конструктор выбрасывает исключение при передаче null в качестве TimeProvider.
    /// </summary>
    [Fact]
    public void ConstructorWithNullTimeProviderShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DepartmentService(_validatorMock.Object, _departmentRepositoryMock.Object, null!,
                _locationRepositoryMock.Object));

        Assert.Equal("timeProvider", exception.ParamName);
    }

    /// <summary>
    ///     Проверяет, что конструктор выбрасывает исключение при передаче null в качестве репозитория локаций.
    /// </summary>
    [Fact]
    public void ConstructorWithNullLocationRepositoryShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DepartmentService(_validatorMock.Object, _departmentRepositoryMock.Object, _timeProvider, null!));

        Assert.Equal("locationRepository", exception.ParamName);
    }

    /// <summary>
    ///     Создает валидный мок-объект <see cref="Location" /> для использования в тестах.
    /// </summary>
    /// <param name="id">Идентификатор локации.</param>
    /// <returns>Валидный объект <see cref="Location" />.</returns>
    private static Location CreateMockLocation(Guid id)
    {
        var name = LocationName.Create("Location Name")
                               .Value;
        var address = Address.Create("Country", "City", null, null)
                             .Value;
        return Location.Create(id, name, address, DateTime.UtcNow)
                       .Value;
    }

    /// <summary>
    ///     Создает валидный мок-объект <see cref="Department" /> для использования в тестах.
    /// </summary>
    /// <param name="id">Идентификатор подразделения.</param>
    /// <returns>Валидный объект <see cref="Department" />.</returns>
    private static Department CreateMockDepartment(Guid id)
    {
        var name = DepartmentName.Create("Test Department")
                                 .Value;
        var slug = DepartmentSlug.Create("test-slug")
                                 .Value;
        var path = DepartmentPath.Create("/test-slug")
                                 .Value;
        var parentInfo = new ParentInfo(id, path);

        return Department.Create(id, name, slug, parentInfo, DateTime.UtcNow)
                         .Value;
    }
}