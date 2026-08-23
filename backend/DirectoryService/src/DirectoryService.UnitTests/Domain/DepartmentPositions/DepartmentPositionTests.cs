using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.UnitTests.Domain.DepartmentPositions;

public sealed class DepartmentPositionTests
{
    [Fact]
    public void CreateWithEmptyPositionIdReturnsValidation()
    {
        var result = DepartmentPosition.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.Empty, DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void CreateWithValidDataReturnsLink()
    {
        var departmentId = Guid.CreateVersion7();
        var positionId = Guid.CreateVersion7();

        var result = DepartmentPosition.Create(Guid.CreateVersion7(), departmentId, positionId, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(departmentId, result.Value.DepartmentId);
        Assert.Equal(positionId, result.Value.PositionId);
    }
}
