using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Features.Departments.Rename;
using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Departments;

public sealed class RenameDepartmentHandlerTests
{
    private readonly Mock<IDepartmentRepository> _repositoryMock = new();
    private readonly Mock<ILogger<RenameDepartmentHandler>> _loggerMock = new();
    private readonly RenameDepartmentHandler _sut;

    public RenameDepartmentHandlerTests()
    {
        _loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new RenameDepartmentHandler(_repositoryMock.Object, TimeProvider.System, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsyncWithValidDataShouldUpdateAndSave()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateDepartmentNameDto("New Department Name");
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(id));

        var result = await _sut.HandleAsync(new RenameDepartmentCommand(id, dto));

        Assert.True(result.IsSuccess);
        DepartmentHandlerTestData.AssertStructuredProperty(_loggerMock, "DepartmentId", id);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidDtoShouldReturnValidationError()
    {
        var dto = new UpdateDepartmentNameDto("");
        var id = Guid.CreateVersion7();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(id));

        var result = await _sut.HandleAsync(new RenameDepartmentCommand(id, dto));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncWithMissingDepartmentShouldReturnNotFound()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateDepartmentNameDto("New Name");
        var expectedError = Error.NotFound("not.found", "not found");
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(expectedError);

        var result = await _sut.HandleAsync(new RenameDepartmentCommand(id, dto));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Type, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidDomainNameShouldReturnValidationError()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateDepartmentNameDto("   ");
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(id));

        var result = await _sut.HandleAsync(new RenameDepartmentCommand(id, dto));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncWhenDataIsValidShouldPrepareChanges()
    {
        var id = Guid.CreateVersion7();
        var dto = new UpdateDepartmentNameDto("New name");
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DepartmentHandlerTestData.CreateDepartment(id));
        var result = await _sut.HandleAsync(new RenameDepartmentCommand(id, dto));

        Assert.True(result.IsSuccess);
    }

}
