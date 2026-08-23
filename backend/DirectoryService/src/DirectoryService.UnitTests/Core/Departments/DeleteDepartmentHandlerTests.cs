using DirectoryService.Core.Departments;
using DirectoryService.Core.Features.Departments.Delete;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;
using Moq;

namespace DirectoryService.UnitTests.Core.Departments;

public sealed class DeleteDepartmentHandlerTests
{
    private readonly Mock<IDepartmentRepository> _repository = new();
    private readonly Mock<ILogger<DeleteDepartmentHandler>> _logger = new();
    private readonly DeleteDepartmentHandler _sut;

    public DeleteDepartmentHandlerTests()
    {
        _logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = new DeleteDepartmentHandler(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task HandleAsyncWithExistingDepartmentRemovesIt()
    {
        var id = Guid.CreateVersion7();
        var department = DepartmentHandlerTestData.CreateDepartment(id);
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(department);

        var result = await _sut.HandleAsync(new DeleteDepartmentCommand(id));

        Assert.True(result.IsSuccess);
        _repository.Verify(x => x.RemoveDepartment(department), Times.Once);
    }

    [Fact]
    public async Task HandleAsyncWithMissingDepartmentReturnsNotFound()
    {
        var id = Guid.CreateVersion7();
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound("department.not.found", "not found"));

        var result = await _sut.HandleAsync(new DeleteDepartmentCommand(id));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
