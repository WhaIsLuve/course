using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Features.Departments.Create;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Departments;

public sealed class CreateDepartmentHandlerTests
{
    private readonly Mock<IValidator<CreateDepartmentDto>> _validatorMock = new();
    private readonly Mock<IDepartmentRepository> _departmentRepositoryMock = new();
    private readonly Mock<ILocationRepository> _locationRepositoryMock = new();
    private readonly Mock<ILogger<CreateDepartmentHandler>> _loggerMock = new();
    private readonly CreateDepartmentHandler _sut;

    public CreateDepartmentHandlerTests()
    {
        _loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new CreateDepartmentHandler(
            _validatorMock.Object,
            _departmentRepositoryMock.Object,
            TimeProvider.System,
            _locationRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsyncWithValidRootDepartmentShouldCreateAndSave()
    {
        var dto = new CreateDepartmentDto("Department Name", "department-slug", null, []);
        SetupValid(dto);

        var result = await _sut.HandleAsync(new CreateDepartmentCommand(dto));

        Assert.True(result.IsSuccess);
        _departmentRepositoryMock.Verify(r => r.AddDepartment(It.IsAny<Department>()), Times.Once);
        _departmentRepositoryMock.Verify(r => r.Save(It.IsAny<CancellationToken>()), Times.Once);
        _departmentRepositoryMock.Verify(r => r.AddDepartmentLocations(It.IsAny<IReadOnlyList<DepartmentLocation>>()), Times.Never);
        DepartmentHandlerTestData.AssertStructuredProperty(_loggerMock, "DepartmentId", result.Value);
    }

    [Fact]
    public async Task HandleAsyncWithChildAndLocationsShouldCreateLinks()
    {
        var parentId = Guid.CreateVersion7();
        var locationId = Guid.CreateVersion7();
        var dto = new CreateDepartmentDto("Department Name", "department-slug", parentId, [locationId]);
        SetupValid(dto);
        _departmentRepositoryMock.Setup(r => r.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(parentId));
        _locationRepositoryMock.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { DepartmentHandlerTestData.CreateLocation(locationId) });

        var result = await _sut.HandleAsync(new CreateDepartmentCommand(dto));

        Assert.True(result.IsSuccess);
        _departmentRepositoryMock.Verify(r => r.AddDepartmentLocations(
            It.Is<IReadOnlyList<DepartmentLocation>>(locations => locations.Count == 1)), Times.Once);
        DepartmentHandlerTestData.AssertStructuredProperty(_loggerMock, "ParentDepartmentId", parentId);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidDtoShouldReturnValidationError()
    {
        var dto = new CreateDepartmentDto("", "", Guid.Empty, [Guid.Empty]);
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateDepartmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
            {
                new("Name", JsonSerializer.Serialize(Error.Validation("code", "Наименование не может быть пустым")))
            }));

        var result = await _sut.HandleAsync(new CreateDepartmentCommand(dto));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        _departmentRepositoryMock.Verify(r => r.AddDepartment(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithMissingParentShouldReturnNotFound()
    {
        var parentId = Guid.CreateVersion7();
        var dto = new CreateDepartmentDto("Name", "slug", parentId, []);
        SetupValid(dto);
        var expectedError = Error.NotFound("not.found", "not found");
        _departmentRepositoryMock.Setup(r => r.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedError);

        var result = await _sut.HandleAsync(new CreateDepartmentCommand(dto));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Type, result.Error.Type);
        _departmentRepositoryMock.Verify(r => r.AddDepartment(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithMissingLocationsShouldReturnNotFound()
    {
        var locationIds = new[] { Guid.CreateVersion7(), Guid.CreateVersion7() };
        var dto = new CreateDepartmentDto("Name", "slug", null, locationIds);
        SetupValid(dto);
        var expectedError = Error.NotFound("locations.not.found", "locations not found");
        _locationRepositoryMock.Setup(r => r.GetByIdsAsync(locationIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedError);

        var result = await _sut.HandleAsync(new CreateDepartmentCommand(dto));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Type, result.Error.Type);
        _departmentRepositoryMock.Verify(r => r.AddDepartment(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWhenSaveFailsShouldReturnFailure()
    {
        var dto = new CreateDepartmentDto("Department", "department", null, []);
        SetupValid(dto);
        var expectedError = Error.Failure("database.save.error", "Save failed");
        _departmentRepositoryMock.Setup(r => r.Save(It.IsAny<CancellationToken>())).ReturnsAsync(expectedError);

        var result = await _sut.HandleAsync(new CreateDepartmentCommand(dto));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }

    private void SetupValid(CreateDepartmentDto dto) =>
        _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
}
