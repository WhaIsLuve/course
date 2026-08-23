using DirectoryService.Contracts.Positions;
using DirectoryService.Core.Features.Positions.Rename;
using DirectoryService.Core.Positions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Positions;

public sealed class RenamePositionHandlerTests
{
    private readonly Mock<IPositionRepository> _repository = new();
    private readonly Mock<ILogger<RenamePositionHandler>> _logger = new();
    private readonly RenamePositionHandler _sut;

    public RenamePositionHandlerTests()
    {
        _logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new RenamePositionHandler(TimeProvider.System, _repository.Object, _logger.Object);
    }

    [Fact]
    public async Task HandleAsyncWithValidDataUpdatesPosition()
    {
        var id = Guid.CreateVersion7();
        var position = PositionHandlerTestData.CreatePosition(id);
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);
        _repository.Setup(x => x.ExistWithSameNameAsync("Architect", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.HandleAsync(new RenamePositionCommand(id, new UpdatePositionDto("Architect")));

        Assert.True(result.IsSuccess);
        Assert.Equal("Architect", position.Name.Value);
    }

    [Fact]
    public async Task HandleAsyncWithExistingNameReturnsConflict()
    {
        var id = Guid.CreateVersion7();
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PositionHandlerTestData.CreatePosition(id));
        _repository.Setup(x => x.ExistWithSameNameAsync("Architect", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.HandleAsync(new RenamePositionCommand(id, new UpdatePositionDto("Architect")));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncWithMissingPositionReturnsNotFound()
    {
        var id = Guid.CreateVersion7();
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound("position.not.found", "not found"));

        var result = await _sut.HandleAsync(new RenamePositionCommand(id, new UpdatePositionDto("Architect")));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
