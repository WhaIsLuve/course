using DirectoryService.Domain.Departments;

namespace DirectoryService.UnitTests.Domain.Departments;

public class DepartmentNameTests
{
    [Theory]
    [InlineData("Sales")]
    [InlineData("HR")]
    [InlineData("A")]
    [InlineData("Department Name")]
    public void CreateValidNameReturnsSuccess(string validName)
    {
        var result = DepartmentName.Create(validName);

        Assert.True(result.IsSuccess);
        Assert.Equal(validName, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateNullOrWhitespaceNameReturnsFailure(string? invalidName)
    {
#pragma warning disable CS8604
        var result = DepartmentName.Create(invalidName);
#pragma warning restore CS8604

        Assert.False(result.IsSuccess);
        var errorMessages = result.Error.Messages;
        Assert.Single(errorMessages);
        Assert.Equal("Наименование указано некоректно", errorMessages.Single().Message);
    }
}