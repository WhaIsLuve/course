using DirectoryService.Core.Departments;
using DirectoryService.Core.Features.Departments.DetachLocation;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Departments;

public sealed class DetachLocationHandlerTests
{
    private readonly Mock<IDepartmentRepository> _departmentRepositoryMock = new();
    private readonly Mock<ILocationRepository> _locationRepositoryMock = new();
    private readonly Mock<ILogger<DetachLocationHandler>> _loggerMock = new();
    private readonly DetachLocationHandler _sut;

    public DetachLocationHandlerTests()
    {
        _loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new DetachLocationHandler(
            _departmentRepositoryMock.Object,
            _locationRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsyncWithValidDataShouldRemoveLink()
    {
        var departmentId = Guid.CreateVersion7();
        var locationId = Guid.CreateVersion7();
        var link = DepartmentLocation.Create(Guid.CreateVersion7(), departmentId, locationId, DateTime.UtcNow).Value;
        SetupEntities(departmentId, locationId);
        _departmentRepositoryMock.Setup(r => r.GetDepartmentLocation(departmentId, locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(link);

        var result = await _sut.HandleAsync(new DetachLocationCommand(departmentId, locationId));

        Assert.True(result.IsSuccess);
        _departmentRepositoryMock.Verify(r => r.RemoveDepartmentLocation(link), Times.Once);
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

        var result = await _sut.HandleAsync(new DetachLocationCommand(departmentId, locationId));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Type, result.Error.Type);
        _departmentRepositoryMock.Verify(r => r.RemoveDepartmentLocation(It.IsAny<DepartmentLocation>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithMissingLocationShouldReturnNotFound()
    {
        var departmentId = Guid.CreateVersion7();
        var locationId = Guid.CreateVersion7();
        SetupEntities(departmentId, locationId);
        var expectedError = Error.NotFound("not.found", "not found");
        _locationRepositoryMock.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedError);

        var result = await _sut.HandleAsync(new DetachLocationCommand(departmentId, locationId));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Type, result.Error.Type);
        _departmentRepositoryMock.Verify(r => r.RemoveDepartmentLocation(It.IsAny<DepartmentLocation>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithMissingLinkShouldReturnNotFound()
    {
        var departmentId = Guid.CreateVersion7();
        var locationId = Guid.CreateVersion7();
        SetupEntities(departmentId, locationId);
        var expectedError = Error.NotFound("not.found", "not found");
        _departmentRepositoryMock.Setup(r => r.GetDepartmentLocation(departmentId, locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedError);

        var result = await _sut.HandleAsync(new DetachLocationCommand(departmentId, locationId));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Type, result.Error.Type);
        _departmentRepositoryMock.Verify(r => r.RemoveDepartmentLocation(It.IsAny<DepartmentLocation>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWhenDataIsValidShouldPrepareChanges()
    {
        var departmentId = Guid.CreateVersion7();
        var locationId = Guid.CreateVersion7();
        var link = DepartmentLocation.Create(Guid.CreateVersion7(), departmentId, locationId, DateTime.UtcNow).Value;
        SetupEntities(departmentId, locationId);
        _departmentRepositoryMock.Setup(r => r.GetDepartmentLocation(departmentId, locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(link);
        var result = await _sut.HandleAsync(new DetachLocationCommand(departmentId, locationId));

        Assert.True(result.IsSuccess);
        _departmentRepositoryMock.Verify(r => r.RemoveDepartmentLocation(link), Times.Once);
    }

    private void SetupEntities(Guid departmentId, Guid locationId)
    {
        _departmentRepositoryMock.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(departmentId));
        _locationRepositoryMock.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateLocation(locationId));
    }
}
