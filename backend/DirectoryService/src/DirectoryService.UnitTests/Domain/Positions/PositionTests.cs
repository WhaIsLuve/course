using DirectoryService.Domain.Positions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.UnitTests.Domain.Positions;

public sealed class PositionTests
{
    [Fact]
    public void CreateWithEmptyIdReturnsValidation()
    {
        var name = PositionName.Create("Developer").Value;

        var result = Position.Create(Guid.Empty, name, DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void UpdateChangesNameAndTimestamp()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(-1);
        var position = Position.Create(Guid.CreateVersion7(), PositionName.Create("Developer").Value, createdAt).Value;
        var result = position.Update(PositionName.Create("Architect").Value, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal("Architect", position.Name.Value);
        Assert.NotNull(position.UpdatedAt);
    }
}
