using System.Text.Json;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Features.Departments.Rename;
using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel.Errors;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Departments;

public sealed class RenameDepartmentHandlerTests
{
    private readonly Mock<IValidator<UpdateDepartmentNameDto>> _validatorMock = new();
    private readonly Mock<IDepartmentRepository> _repositoryMock = new();
    private readonly Mock<ILogger<RenameDepartmentHandler>> _loggerMock = new();
    private readonly RenameDepartmentHandler _sut;

    public RenameDepartmentHandlerTests()
    {
        _loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new RenameDepartmentHandler(_validatorMock.Object, _repositoryMock.Object, TimeProvider.System, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsyncWithValidDataShouldUpdateAndSave()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateDepartmentNameDto("New Department Name");
        SetupValid(dto);
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(id));

        var result = await _sut.HandleAsync(new RenameDepartmentCommand(id, dto));

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.Save(It.IsAny<CancellationToken>()), Times.Once);
        DepartmentHandlerTestData.AssertStructuredProperty(_loggerMock, "DepartmentId", id);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidDtoShouldReturnValidationError()
    {
        var dto = new UpdateDepartmentNameDto("");
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateDepartmentNameDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
            {
                new("Name", JsonSerializer.Serialize(Error.Validation("code", "Наименование не может быть пустым")))
            }));

        var result = await _sut.HandleAsync(new RenameDepartmentCommand(Guid.CreateVersion7(), dto));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithMissingDepartmentShouldReturnNotFound()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateDepartmentNameDto("New Name");
        SetupValid(dto);
        var expectedError = Error.NotFound("not.found", "not found");
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(expectedError);

        var result = await _sut.HandleAsync(new RenameDepartmentCommand(id, dto));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Type, result.Error.Type);
        _repositoryMock.Verify(r => r.Save(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidDomainNameShouldReturnValidationError()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateDepartmentNameDto("   ");
        SetupValid(dto);
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(id));

        var result = await _sut.HandleAsync(new RenameDepartmentCommand(id, dto));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        _repositoryMock.Verify(r => r.Save(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsyncWhenSaveFailsShouldReturnFailure()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateDepartmentNameDto("New name");
        SetupValid(dto);
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(id));
        var expectedError = Error.Failure("database.save.error", "Save failed");
        _repositoryMock.Setup(r => r.Save(It.IsAny<CancellationToken>())).ReturnsAsync(expectedError);

        var result = await _sut.HandleAsync(new RenameDepartmentCommand(id, dto));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }

    private void SetupValid(UpdateDepartmentNameDto dto) =>
        _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
}
