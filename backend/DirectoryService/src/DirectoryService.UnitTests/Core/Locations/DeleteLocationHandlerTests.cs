using DirectoryService.Core.Features.Locations.Delete;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Locations;

public sealed class DeleteLocationHandlerTests
{
    private readonly Mock<ILocationRepository> _repository = new();
    private readonly Mock<ILogger<DeleteLocationHandler>> _logger = new();
    private readonly DeleteLocationHandler _sut;

    public DeleteLocationHandlerTests()
    {
        _logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new DeleteLocationHandler(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task HandleAsyncWithUnlinkedLocationRemovesIt()
    {
        var id = Guid.CreateVersion7();
        var location = LocationHandlerTestData.CreateLocation(id);
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(location);
        _repository.Setup(x => x.HasDepartmentLinksAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.HandleAsync(new DeleteLocationCommand(id));

        Assert.True(result.IsSuccess);
        _repository.Verify(x => x.Remove(location), Times.Once);
    }

    [Fact]
    public async Task HandleAsyncWithLinkedLocationReturnsConflict()
    {
        var id = Guid.CreateVersion7();
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LocationHandlerTestData.CreateLocation(id));
        _repository.Setup(x => x.HasDepartmentLinksAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.HandleAsync(new DeleteLocationCommand(id));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _repository.Verify(x => x.Remove(It.IsAny<Location>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithMissingLocationReturnsNotFound()
    {
        var id = Guid.CreateVersion7();
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound("location.not.found", "not found"));

        var result = await _sut.HandleAsync(new DeleteLocationCommand(id));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
