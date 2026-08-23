using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Features.Locations.Update;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Locations;

public sealed class UpdateLocationHandlerTests
{
    private readonly Mock<ILocationRepository> _repositoryMock = new();
    private readonly Mock<ILogger<UpdateLocationHandler>> _loggerMock = new();
    private readonly UpdateLocationHandler _sut;

    public UpdateLocationHandlerTests()
    {
        _loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new UpdateLocationHandler(
            TimeProvider.System,
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsyncWithChangedNameShouldCheckUniqueness()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateLocationDto("New Location Name", new AddressDto("Country", "City", "Street", "Building"));
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LocationHandlerTestData.CreateLocation(id));
        _repositoryMock.Setup(r => r.ExistWithSameNameAsync(dto.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.HandleAsync(new UpdateLocationCommand(id, dto));

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.ExistWithSameNameAsync(dto.Name, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsyncWithSameNameShouldNotCheckUniqueness()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateLocationDto("Location Name", new AddressDto("New Country", "City", "Street", "Building"));
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LocationHandlerTestData.CreateLocation(id));

        var result = await _sut.HandleAsync(new UpdateLocationCommand(id, dto));

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidDtoShouldReturnValidationError()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateLocationDto("", new AddressDto("", "", "", ""));
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LocationHandlerTestData.CreateLocation(id));

        var result = await _sut.HandleAsync(new UpdateLocationCommand(id, dto));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncWithMissingLocationShouldReturnNotFound()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateLocationDto("New Name", new AddressDto("Country", "City", "Street", "Building"));
        var expectedError = Error.NotFound("not.found", "not found");
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(expectedError);

        var result = await _sut.HandleAsync(new UpdateLocationCommand(id, dto));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Type, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidDomainNameShouldReturnValidationError()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateLocationDto(new string('a', LocationName.MaxLength + 1), new AddressDto("Country", "City", "Street", "Building"));
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LocationHandlerTestData.CreateLocation(id));

        var result = await _sut.HandleAsync(new UpdateLocationCommand(id, dto));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidDomainAddressShouldReturnValidationError()
    {
        var id = Guid.CreateVersion7();
        var invalidCountry = new string('a', Address.CountryMaxLength + 1);
        var dto = new UpdateLocationDto("New Name", new AddressDto(invalidCountry, "City", "Street", "Building"));
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LocationHandlerTestData.CreateLocation(id));

        var result = await _sut.HandleAsync(new UpdateLocationCommand(id, dto));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncWithExistingNameShouldReturnConflict()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateLocationDto("Existing Name", new AddressDto("Country", "City", "Street", "Building"));
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LocationHandlerTestData.CreateLocation(id));
        _repositoryMock.Setup(r => r.ExistWithSameNameAsync(dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.HandleAsync(new UpdateLocationCommand(id, dto));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncWhenDataIsValidShouldPrepareChanges()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateLocationDto("Location Name", new AddressDto("Country", "New City", null, null));
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LocationHandlerTestData.CreateLocation(id));
        var result = await _sut.HandleAsync(new UpdateLocationCommand(id, dto));

        Assert.True(result.IsSuccess);
    }

}
