using DirectoryService.Contracts.Positions;
using DirectoryService.Core.Features.Positions.Create;
using DirectoryService.Core.Positions;
using DirectoryService.Domain.Positions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Positions;

public sealed class CreatePositionHandlerTests
{
    private readonly Mock<IPositionRepository> _repository = new();
    private readonly Mock<ILogger<CreatePositionHandler>> _logger = new();
    private readonly CreatePositionHandler _sut;

    public CreatePositionHandlerTests()
    {
        _logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new CreatePositionHandler(TimeProvider.System, _repository.Object, _logger.Object);
    }

    [Fact]
    public async Task HandleAsyncWithValidDataAddsPosition()
    {
        _repository.Setup(x => x.ExistWithSameNameAsync("Developer", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.HandleAsync(new CreatePositionCommand(new CreatePositionDto("Developer")));

        Assert.True(result.IsSuccess);
        _repository.Verify(x => x.Add(It.IsAny<Position>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsyncWithExistingNameReturnsConflict()
    {
        _repository.Setup(x => x.ExistWithSameNameAsync("Developer", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.HandleAsync(new CreatePositionCommand(new CreatePositionDto("Developer")));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _repository.Verify(x => x.Add(It.IsAny<Position>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidNameReturnsValidation()
    {
        _repository.Setup(x => x.ExistWithSameNameAsync("", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.HandleAsync(new CreatePositionCommand(new CreatePositionDto("")));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        _repository.Verify(x => x.Add(It.IsAny<Position>()), Times.Never);
    }
}
