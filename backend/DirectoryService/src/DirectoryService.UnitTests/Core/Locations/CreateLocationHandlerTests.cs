using System.Text.Json;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Features.Locations.Create;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Locations;

public sealed class CreateLocationHandlerTests
{
    private readonly Mock<IValidator<CreateLocationDto>> _validatorMock = new();
    private readonly Mock<ILocationRepository> _repositoryMock = new();
    private readonly Mock<ILogger<CreateLocationHandler>> _loggerMock = new();
    private readonly CreateLocationHandler _sut;

    public CreateLocationHandlerTests()
    {
        _loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new CreateLocationHandler(
            TimeProvider.System,
            _repositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsyncWithValidDataShouldReturnIdAndCallRepository()
    {
        var dto = new CreateLocationDto("Test Location", new AddressDto("Country", "City", "Street", "Building"));
        SetupValid(dto);
        _repositoryMock.Setup(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.HandleAsync(new CreateLocationCommand(dto));

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.Add(It.IsAny<Location>()), Times.Once);
        LocationHandlerTestData.AssertStructuredProperty(_loggerMock, "LocationId", result.Value);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidDtoShouldReturnValidationError()
    {
        var dto = new CreateLocationDto("", new AddressDto("", "", "", ""));
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateLocationDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
            {
                new("Name", JsonSerializer.Serialize(Error.Validation("code", "Имя обязательное поле")))
            }));

        var result = await _sut.HandleAsync(new CreateLocationCommand(dto));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        _repositoryMock.Verify(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.Add(It.IsAny<Location>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithExistingNameShouldReturnConflictError()
    {
        var dto = new CreateLocationDto("Existing Location", new AddressDto("Country", "City", "Street", "Building"));
        SetupValid(dto);
        _repositoryMock.Setup(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.HandleAsync(new CreateLocationCommand(dto));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _repositoryMock.Verify(r => r.Add(It.IsAny<Location>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidAddressShouldReturnValidationError()
    {
        var invalidCountry = new string('a', Address.CountryMaxLength + 1);
        var dto = new CreateLocationDto("Test Location", new AddressDto(invalidCountry, "City", "Street", "Building"));
        SetupValid(dto);
        _repositoryMock.Setup(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.HandleAsync(new CreateLocationCommand(dto));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        _repositoryMock.Verify(r => r.Add(It.IsAny<Location>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidLocationNameShouldReturnValidationError()
    {
        var invalidName = new string('a', LocationName.MaxLength + 1);
        var dto = new CreateLocationDto(invalidName, new AddressDto("Country", "City", "Street", "Building"));
        SetupValid(dto);
        _repositoryMock.Setup(r => r.ExistWithSameNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.HandleAsync(new CreateLocationCommand(dto));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        _repositoryMock.Verify(r => r.Add(It.IsAny<Location>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWhenSaveFailsShouldReturnFailure()
    {
        var dto = new CreateLocationDto("Location", new AddressDto("Country", "City", null, null));
        SetupValid(dto);
        _repositoryMock.Setup(r => r.ExistWithSameNameAsync(dto.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var expectedError = Error.Failure("database.save.error", "Save failed");
        _repositoryMock.Setup(r => r.Save(It.IsAny<CancellationToken>())).ReturnsAsync(expectedError);

        var result = await _sut.HandleAsync(new CreateLocationCommand(dto));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
        _repositoryMock.Verify(r => r.Add(It.IsAny<Location>()), Times.Once);
    }

    private void SetupValid(CreateLocationDto dto) =>
        _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
}
