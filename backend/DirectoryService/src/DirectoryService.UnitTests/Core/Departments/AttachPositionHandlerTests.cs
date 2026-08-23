using DirectoryService.Core.Departments;
using DirectoryService.Core.Features.Departments.AttachPosition;
using DirectoryService.Core.Positions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.SharedKernel.Errors;
using DirectoryService.UnitTests.Core.Positions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Departments;

public sealed class AttachPositionHandlerTests
{
    private readonly Mock<IDepartmentRepository> _departmentRepository = new();
    private readonly Mock<IPositionRepository> _positionRepository = new();
    private readonly Mock<ILogger<AttachPositionHandler>> _logger = new();
    private readonly AttachPositionHandler _sut;

    public AttachPositionHandlerTests()
    {
        _logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new AttachPositionHandler(_departmentRepository.Object, _positionRepository.Object,
            TimeProvider.System, _logger.Object);
    }

    [Fact]
    public async Task HandleAsyncWithValidDataAddsLink()
    {
        var departmentId = Guid.CreateVersion7();
        var positionId = Guid.CreateVersion7();
        _departmentRepository.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(departmentId));
        _positionRepository.Setup(x => x.GetByIdAsync(positionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PositionHandlerTestData.CreatePosition(positionId));
        _departmentRepository.Setup(x => x.ExistDepartmentPosition(departmentId, positionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.HandleAsync(new AttachPositionCommand(departmentId, positionId));

        Assert.True(result.IsSuccess);
        _departmentRepository.Verify(x => x.AddDepartmentPosition(It.Is<DepartmentPosition>(link =>
            link.DepartmentId == departmentId && link.PositionId == positionId)), Times.Once);
    }

    [Fact]
    public async Task HandleAsyncWithExistingLinkReturnsConflict()
    {
        var departmentId = Guid.CreateVersion7();
        var positionId = Guid.CreateVersion7();
        _departmentRepository.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(departmentId));
        _positionRepository.Setup(x => x.GetByIdAsync(positionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PositionHandlerTestData.CreatePosition(positionId));
        _departmentRepository.Setup(x => x.ExistDepartmentPosition(departmentId, positionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.HandleAsync(new AttachPositionCommand(departmentId, positionId));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _departmentRepository.Verify(x => x.AddDepartmentPosition(It.IsAny<DepartmentPosition>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithMissingPositionReturnsNotFound()
    {
        var departmentId = Guid.CreateVersion7();
        var positionId = Guid.CreateVersion7();
        _departmentRepository.Setup(x => x.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(departmentId));
        _positionRepository.Setup(x => x.GetByIdAsync(positionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound("position.not.found", "not found"));

        var result = await _sut.HandleAsync(new AttachPositionCommand(departmentId, positionId));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
