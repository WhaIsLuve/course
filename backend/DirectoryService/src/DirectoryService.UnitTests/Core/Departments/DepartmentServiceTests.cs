using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;
using DirectoryService.SharedKernel.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ValidationException = DirectoryService.SharedKernel.Exceptions.ValidationException;

namespace DirectoryService.UnitTests.Core.Departments;

public class DepartmentServiceTests
{
	private readonly Mock<IValidator<CreateDepartmentDto>> _createValidatorMock;
	private readonly Mock<IDepartmentRepository> _departmentRepositoryMock;
	private readonly Mock<ILocationRepository> _locationRepositoryMock;
	private readonly DepartmentService _sut;
	private readonly TimeProvider _timeProvider;
	private readonly Mock<IValidator<UpdateDepartmentNameDto>> _updateNameValidatorMock;

	public DepartmentServiceTests()
	{
		_updateNameValidatorMock = new Mock<IValidator<UpdateDepartmentNameDto>>();
		_createValidatorMock = new Mock<IValidator<CreateDepartmentDto>>();
		_departmentRepositoryMock = new Mock<IDepartmentRepository>();
		_locationRepositoryMock = new Mock<ILocationRepository>();
		_timeProvider = TimeProvider.System;
		_sut = new DepartmentService(
			_createValidatorMock.Object,
			_departmentRepositoryMock.Object,
			_timeProvider,
			_locationRepositoryMock.Object,
			_updateNameValidatorMock.Object);
	}

	[Fact]
	public async Task CreateAsyncWithValidRootDepartmentAndNoLocationsShouldReturnIdAndCallRepository()
	{
		// Arrange
		var dto = new CreateDepartmentDto("Department Name", "department-slug", null, new List<Guid>());

		_createValidatorMock
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

		_createValidatorMock
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

	[Fact]
	public async Task CreateAsyncWithInvalidDtoShouldReturnValidationError()
	{
		// Arrange
		var dto = new CreateDepartmentDto("", "", Guid.Empty, new List<Guid> { Guid.Empty });
		var validationFailures = new List<ValidationFailure>
		{
			new("Name", "Наименование не может быть пустым")
		};

		_createValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<CreateDepartmentDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult(validationFailures));

		// Act
		var result = await _sut.CreateAsync(dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(ErrorType.Validation, result.Error.Type);
		_departmentRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_departmentRepositoryMock.Verify(r => r.AddDepartment(It.IsAny<Department>()), Times.Never);
	}

	[Fact]
	public async Task CreateAsyncWithNonExistentParentShouldReturnNotFoundError()
	{
		// Arrange
		var parentId = Guid.NewGuid();
		var dto = new CreateDepartmentDto("Name", "slug", parentId, new List<Guid>());
		var expectedError = Error.NotFound("not found", "not found");

		_createValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<CreateDepartmentDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_departmentRepositoryMock
			.Setup(r => r.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.CreateAsync(dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError.Type, result.Error.Type);
		_departmentRepositoryMock.Verify(r => r.AddDepartment(It.IsAny<Department>()), Times.Never);
	}

	[Fact]
	public async Task CreateAsyncWithNonExistentLocationsShouldReturnNotFoundError()
	{
		// Arrange
		var locationId1 = Guid.NewGuid();
		var locationId2 = Guid.NewGuid();
		var dto = new CreateDepartmentDto("Name", "slug", null, new List<Guid> { locationId1, locationId2 });
		var expectedError = Error.NotFound("not found", "not found");

		_createValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<CreateDepartmentDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_locationRepositoryMock
			.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.CreateAsync(dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError.Type, result.Error.Type);
		_departmentRepositoryMock.Verify(r => r.AddDepartment(It.IsAny<Department>()), Times.Never);
	}






	[Fact]
	public async Task UpdateNameAsyncWithValidDataShouldUpdateNameAndCallSave()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var dto = new UpdateDepartmentNameDto("New Department Name");
		var department = CreateMockDepartment(departmentId);

		_updateNameValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<UpdateDepartmentNameDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_departmentRepositoryMock
			.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(department);

		// Act
		await _sut.UpdateNameAsync(departmentId, dto, CancellationToken.None);

		// Assert
		_departmentRepositoryMock.Verify(r => r.Save(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task UpdateNameAsyncWithInvalidDtoShouldReturnValidationError()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var dto = new UpdateDepartmentNameDto("");
		var validationFailures = new List<ValidationFailure>
		{
			new("Name", "Наименование не может быть пустым")
		};

		_updateNameValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<UpdateDepartmentNameDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult(validationFailures));

		// Act
		var result = await _sut.UpdateNameAsync(departmentId, dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(ErrorType.Validation, result.Error.Type);
		_departmentRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task UpdateNameAsyncWithNonExistentDepartmentShouldReturnNotFoundError()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var dto = new UpdateDepartmentNameDto("New Name");
		var expectedError = Error.NotFound("not found", "not found");

		_updateNameValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<UpdateDepartmentNameDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_departmentRepositoryMock
			.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);
		// Act
		var result = await _sut.UpdateNameAsync(departmentId, dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError.Type, result.Error.Type);
		_departmentRepositoryMock.Verify(r => r.Save(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task UpdateNameAsyncWithInvalidDomainNameShouldReturnValidationError()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var dto = new UpdateDepartmentNameDto(
			"   "); // Пустая строка после trim, пройдет DTO валидатор, но упадет в домене
		var department = CreateMockDepartment(departmentId);

		_updateNameValidatorMock
			.Setup(v => v.ValidateAsync(It.IsAny<UpdateDepartmentNameDto>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());

		_departmentRepositoryMock
			.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(department);

		// Act
		var result = await _sut.UpdateNameAsync(departmentId, dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(ErrorType.Validation, result.Error.Type);
		_departmentRepositoryMock.Verify(r => r.Save(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task AttachLocationWithValidDataShouldAddDepartmentLocation()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var locationId = Guid.NewGuid();
		var department = CreateMockDepartment(departmentId);
		var location = CreateMockLocation(locationId);

		_departmentRepositoryMock
			.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(department);

		_locationRepositoryMock
			.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(location);

		_departmentRepositoryMock
			.Setup(r => r.ExistDepartmentLocation(departmentId, locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		await _sut.AttachLocation(departmentId, locationId, CancellationToken.None);

		// Assert
		_departmentRepositoryMock.Verify(
			r => r.AddDepartmentLocations(It.IsAny<IReadOnlyList<DepartmentLocation>>()),
			Times.Once);
	}

	[Fact]
	public async Task AttachLocationWithNonExistentDepartmentShouldReturnNotFoundError()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var locationId = Guid.NewGuid();
		var expectedError = Error.NotFound("not found", "not found");

		_departmentRepositoryMock
			.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.AttachLocation(departmentId, locationId, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError.Type, result.Error.Type);
		_departmentRepositoryMock.Verify(
			r => r.AddDepartmentLocations(It.IsAny<IReadOnlyList<DepartmentLocation>>()),
			Times.Never);
	}

	[Fact]
	public async Task AttachLocationWithNonExistentLocationShouldReturnNotFoundError()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var locationId = Guid.NewGuid();
		var department = CreateMockDepartment(departmentId);
		var expectedError = Error.NotFound("not found", "not found");

		_departmentRepositoryMock
			.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(department);

		_locationRepositoryMock
			.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.AttachLocation(departmentId, locationId, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError.Type, result.Error.Type);
		_departmentRepositoryMock.Verify(
			r => r.AddDepartmentLocations(It.IsAny<IReadOnlyList<DepartmentLocation>>()),
			Times.Never);
	}

	[Fact]
	public async Task AttachLocationWithExistingLinkShouldReturnConflictError()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var locationId = Guid.NewGuid();
		var department = CreateMockDepartment(departmentId);
		var location = CreateMockLocation(locationId);

		_departmentRepositoryMock
			.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(department);

		_locationRepositoryMock
			.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(location);

		_departmentRepositoryMock
			.Setup(r => r.ExistDepartmentLocation(departmentId, locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		var result = await _sut.AttachLocation(departmentId, locationId, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(ErrorType.Conflict, result.Error.Type);
		_departmentRepositoryMock.Verify(
			r => r.AddDepartmentLocations(It.IsAny<IReadOnlyList<DepartmentLocation>>()),
			Times.Never);
	}

	[Fact]
	public async Task DetachLocationWithValidDataShouldRemoveDepartmentLocation()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var locationId = Guid.NewGuid();
		var department = CreateMockDepartment(departmentId);
		var location = CreateMockLocation(locationId);
		var departmentLocation = DepartmentLocation.Create(
			Guid.NewGuid(), departmentId, locationId, DateTime.UtcNow).Value;

		_departmentRepositoryMock
			.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(department);

		_locationRepositoryMock
			.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(location);

		_departmentRepositoryMock
			.Setup(r => r.GetDepartmentLocation(departmentId, locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(departmentLocation);

		// Act
		await _sut.DetachLocation(departmentId, locationId, CancellationToken.None);

		// Assert
		_departmentRepositoryMock.Verify(
			r => r.RemoveDepartmentLocation(departmentLocation),
			Times.Once);
	}

	[Fact]
	public async Task DetachLocationWithNonExistentDepartmentShouldReturnNotFoundError()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var locationId = Guid.NewGuid();
		var expectedError = Error.NotFound("not found", "not found");

		_departmentRepositoryMock
			.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.DetachLocation(departmentId, locationId, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError.Type, result.Error.Type);
		_departmentRepositoryMock.Verify(
			r => r.RemoveDepartmentLocation(It.IsAny<DepartmentLocation>()),
			Times.Never);
	}

	[Fact]
	public async Task DetachLocationWithNonExistentLocationShouldReturnNotFoundError()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var locationId = Guid.NewGuid();
		var department = CreateMockDepartment(departmentId);
		var expectedError = Error.NotFound("not found", "not found");

		_departmentRepositoryMock
			.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(department);
		_locationRepositoryMock
			.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.DetachLocation(departmentId, locationId, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError.Type, result.Error.Type);
		_departmentRepositoryMock.Verify(
			r => r.RemoveDepartmentLocation(It.IsAny<DepartmentLocation>()),
			Times.Never);
	}

	[Fact]
	public async Task DetachLocationWithNonExistentLinkShouldReturnNotFoundError()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var locationId = Guid.NewGuid();
		var department = CreateMockDepartment(departmentId);
		var location = CreateMockLocation(locationId);
		var expectedError = Error.NotFound("not found", "not found");

		_departmentRepositoryMock
			.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(department);

		_locationRepositoryMock
			.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(location);

		_departmentRepositoryMock
			.Setup(r => r.GetDepartmentLocation(departmentId, locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.DetachLocation(departmentId, locationId, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError.Type, result.Error.Type);
		_departmentRepositoryMock.Verify(
			r => r.RemoveDepartmentLocation(It.IsAny<DepartmentLocation>()),
			Times.Never);
	}

	[Fact]
	public async Task CreateAsyncWhenSaveFailsShouldReturnFailure()
	{
		// Arrange
		var dto = new CreateDepartmentDto("Department", "department", null, []);
		var expectedError = Error.Failure("database.save.error", "Save failed");
		_createValidatorMock
			.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());
		_departmentRepositoryMock
			.Setup(r => r.Save(It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.CreateAsync(dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError, result.Error);
		_departmentRepositoryMock.Verify(r => r.AddDepartment(It.IsAny<Department>()), Times.Once);
	}

	[Fact]
	public async Task UpdateNameAsyncWhenSaveFailsShouldReturnFailure()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var department = CreateMockDepartment(departmentId);
		var dto = new UpdateDepartmentNameDto("New name");
		var expectedError = Error.Failure("database.save.error", "Save failed");
		_updateNameValidatorMock
			.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ValidationResult());
		_departmentRepositoryMock
			.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(department);
		_departmentRepositoryMock
			.Setup(r => r.Save(It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.UpdateNameAsync(departmentId, dto, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError, result.Error);
	}

	[Fact]
	public async Task AttachLocationWhenSaveFailsShouldReturnFailure()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var locationId = Guid.NewGuid();
		var expectedError = Error.Failure("database.save.error", "Save failed");
		_departmentRepositoryMock.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(CreateMockDepartment(departmentId));
		_locationRepositoryMock.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(CreateMockLocation(locationId));
		_departmentRepositoryMock.Setup(r => r.ExistDepartmentLocation(departmentId, locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);
		_departmentRepositoryMock.Setup(r => r.Save(It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.AttachLocation(departmentId, locationId, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError, result.Error);
	}

	[Fact]
	public async Task DetachLocationWhenSaveFailsShouldReturnFailure()
	{
		// Arrange
		var departmentId = Guid.NewGuid();
		var locationId = Guid.NewGuid();
		var expectedError = Error.Failure("database.save.error", "Save failed");
		var link = DepartmentLocation.Create(Guid.NewGuid(), departmentId, locationId, DateTime.UtcNow).Value;
		_departmentRepositoryMock.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(CreateMockDepartment(departmentId));
		_locationRepositoryMock.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(CreateMockLocation(locationId));
		_departmentRepositoryMock.Setup(r => r.GetDepartmentLocation(departmentId, locationId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(link);
		_departmentRepositoryMock.Setup(r => r.Save(It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedError);

		// Act
		var result = await _sut.DetachLocation(departmentId, locationId, CancellationToken.None);

		// Assert
		Assert.True(result.IsFailure);
		Assert.Equal(expectedError, result.Error);
		_departmentRepositoryMock.Verify(r => r.RemoveDepartmentLocation(link), Times.Once);
	}

	private static Location CreateMockLocation(Guid id)
	{
		var name = LocationName.Create("Location Name")
			.Value;
		var address = Address.Create("Country", "City", null, null)
			.Value;
		return Location.Create(id, name, address, DateTime.UtcNow)
			.Value;
	}

	private static Department CreateMockDepartment(Guid id)
	{
		var name = DepartmentName.Create("Test Department")
			.Value;
		var slug = DepartmentSlug.Create("test-slug")
			.Value;
		var path = DepartmentPath.Create("/test-slug")
			.Value;
		var parentInfo = new ParentInfo(Guid.CreateVersion7(), path);

		return Department.Create(id, name, slug, parentInfo, DateTime.UtcNow)
			.Value;
	}
}
