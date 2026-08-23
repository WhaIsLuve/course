using DirectoryService.Core.Departments;
using DirectoryService.Core.Features.Departments.AttachLocation;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Departments;

public sealed class AttachLocationHandlerTests
{
    private readonly Mock<IDepartmentRepository> _departmentRepositoryMock = new();
    private readonly Mock<ILocationRepository> _locationRepositoryMock = new();
    private readonly Mock<ILogger<AttachLocationHandler>> _loggerMock = new();
    private readonly AttachLocationHandler _sut;

    public AttachLocationHandlerTests()
    {
        _loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new AttachLocationHandler(
            _departmentRepositoryMock.Object,
            _locationRepositoryMock.Object,
            TimeProvider.System,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsyncWithValidDataShouldAddLink()
    {
        var departmentId = Guid.CreateVersion7();
        var locationId = Guid.CreateVersion7();
        SetupEntities(departmentId, locationId);
        _departmentRepositoryMock.Setup(r => r.ExistDepartmentLocation(departmentId, locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.HandleAsync(new AttachLocationCommand(departmentId, locationId));

        Assert.True(result.IsSuccess);
        _departmentRepositoryMock.Verify(r => r.AddDepartmentLocations(It.IsAny<IReadOnlyList<DepartmentLocation>>()), Times.Once);
        DepartmentHandlerTestData.AssertStructuredProperty(_loggerMock, "DepartmentId", departmentId);
        DepartmentHandlerTestData.AssertStructuredProperty(_loggerMock, "LocationId", locationId);
    }

    [Fact]
    public async Task HandleAsyncWithMissingDepartmentShouldReturnNotFound()
    {
        var departmentId = Guid.CreateVersion7();
        var locationId = Guid.CreateVersion7();
        var expectedError = Error.NotFound("not.found", "not found");
        _departmentRepositoryMock.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedError);

        var result = await _sut.HandleAsync(new AttachLocationCommand(departmentId, locationId));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Type, result.Error.Type);
        _departmentRepositoryMock.Verify(r => r.AddDepartmentLocations(It.IsAny<IReadOnlyList<DepartmentLocation>>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithMissingLocationShouldReturnNotFound()
    {
        var departmentId = Guid.CreateVersion7();
        var locationId = Guid.CreateVersion7();
        var expectedError = Error.NotFound("not.found", "not found");
        _departmentRepositoryMock.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(departmentId));
        _locationRepositoryMock.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedError);

        var result = await _sut.HandleAsync(new AttachLocationCommand(departmentId, locationId));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Type, result.Error.Type);
        _departmentRepositoryMock.Verify(r => r.AddDepartmentLocations(It.IsAny<IReadOnlyList<DepartmentLocation>>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithExistingLinkShouldReturnConflict()
    {
        var departmentId = Guid.CreateVersion7();
        var locationId = Guid.CreateVersion7();
        SetupEntities(departmentId, locationId);
        _departmentRepositoryMock.Setup(r => r.ExistDepartmentLocation(departmentId, locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.HandleAsync(new AttachLocationCommand(departmentId, locationId));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _departmentRepositoryMock.Verify(r => r.AddDepartmentLocations(It.IsAny<IReadOnlyList<DepartmentLocation>>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWhenSaveFailsShouldReturnFailure()
    {
        var departmentId = Guid.CreateVersion7();
        var locationId = Guid.CreateVersion7();
        SetupEntities(departmentId, locationId);
        _departmentRepositoryMock.Setup(r => r.ExistDepartmentLocation(departmentId, locationId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var expectedError = Error.Failure("database.save.error", "Save failed");
        _departmentRepositoryMock.Setup(r => r.Save(It.IsAny<CancellationToken>())).ReturnsAsync(expectedError);

        var result = await _sut.HandleAsync(new AttachLocationCommand(departmentId, locationId));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }

    private void SetupEntities(Guid departmentId, Guid locationId)
    {
        _departmentRepositoryMock.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(departmentId));
        _locationRepositoryMock.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateLocation(locationId));
    }
}
