using DirectoryService.Core.Features.Positions.Delete;
using DirectoryService.Core.Positions;
using DirectoryService.Domain.Positions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Positions;

public sealed class DeletePositionHandlerTests
{
    private readonly Mock<IPositionRepository> _repository = new();
    private readonly Mock<ILogger<DeletePositionHandler>> _logger = new();
    private readonly DeletePositionHandler _sut;

    public DeletePositionHandlerTests()
    {
        _logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new DeletePositionHandler(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task HandleAsyncWithUnlinkedPositionRemovesIt()
    {
        var id = Guid.CreateVersion7();
        var position = PositionHandlerTestData.CreatePosition(id);
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(position);
        _repository.Setup(x => x.HasDepartmentLinksAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.HandleAsync(new DeletePositionCommand(id));

        Assert.True(result.IsSuccess);
        _repository.Verify(x => x.Remove(position), Times.Once);
    }

    [Fact]
    public async Task HandleAsyncWithLinkedPositionReturnsConflict()
    {
        var id = Guid.CreateVersion7();
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PositionHandlerTestData.CreatePosition(id));
        _repository.Setup(x => x.HasDepartmentLinksAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.HandleAsync(new DeletePositionCommand(id));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _repository.Verify(x => x.Remove(It.IsAny<Position>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithMissingPositionReturnsNotFound()
    {
        var id = Guid.CreateVersion7();
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound("position.not.found", "not found"));

        var result = await _sut.HandleAsync(new DeletePositionCommand(id));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
