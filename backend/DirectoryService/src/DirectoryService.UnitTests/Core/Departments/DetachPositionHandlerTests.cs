using DirectoryService.Core.Departments;
using DirectoryService.Core.Features.Departments.DetachPosition;
using DirectoryService.Core.Positions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.SharedKernel.Errors;
using DirectoryService.UnitTests.Core.Positions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Departments;

public sealed class DetachPositionHandlerTests
{
    private readonly Mock<IDepartmentRepository> _departmentRepository = new();
    private readonly Mock<IPositionRepository> _positionRepository = new();
    private readonly Mock<ILogger<DetachPositionHandler>> _logger = new();
    private readonly DetachPositionHandler _sut;

    public DetachPositionHandlerTests()
    {
        _logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new DetachPositionHandler(_departmentRepository.Object, _positionRepository.Object, _logger.Object);
    }

    [Fact]
    public async Task HandleAsyncWithExistingLinkRemovesIt()
    {
        var departmentId = Guid.CreateVersion7();
        var positionId = Guid.CreateVersion7();
        var link = DepartmentPosition.Create(Guid.CreateVersion7(), departmentId, positionId, DateTime.UtcNow).Value;
        _departmentRepository.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(departmentId));
        _positionRepository.Setup(x => x.GetByIdAsync(positionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PositionHandlerTestData.CreatePosition(positionId));
        _departmentRepository.Setup(x => x.GetDepartmentPosition(departmentId, positionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(link);

        var result = await _sut.HandleAsync(new DetachPositionCommand(departmentId, positionId));

        Assert.True(result.IsSuccess);
        _departmentRepository.Verify(x => x.RemoveDepartmentPosition(link), Times.Once);
    }

    [Fact]
    public async Task HandleAsyncWithMissingLinkReturnsNotFound()
    {
        var departmentId = Guid.CreateVersion7();
        var positionId = Guid.CreateVersion7();
        _departmentRepository.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(departmentId));
        _positionRepository.Setup(x => x.GetByIdAsync(positionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PositionHandlerTestData.CreatePosition(positionId));
        _departmentRepository.Setup(x => x.GetDepartmentPosition(departmentId, positionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound("department.position.not.found", "not found"));

        var result = await _sut.HandleAsync(new DetachPositionCommand(departmentId, positionId));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        _departmentRepository.Verify(x => x.RemoveDepartmentPosition(It.IsAny<DepartmentPosition>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithMissingDepartmentReturnsNotFound()
    {
        var departmentId = Guid.CreateVersion7();
        var positionId = Guid.CreateVersion7();
        _departmentRepository.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound("department.not.found", "not found"));

        var result = await _sut.HandleAsync(new DetachPositionCommand(departmentId, positionId));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
