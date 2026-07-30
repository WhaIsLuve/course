using DirectoryService.Domain.Locations;

namespace DirectoryService.UnitTests.Domain.Locations;

public class LocationNameTests
{
    [Theory]
    [InlineData("Moscow Office")]
    [InlineData("A")]
    [InlineData("Headquarters")]
    public void CreateValidNameReturnsSuccess(string validName)
    {
        var result = LocationName.Create(validName);

        Assert.True(result.IsSuccess);
        Assert.Equal(validName, result.Value.Value);
    }

    [Fact]
    public void CreateNullNameReturnsFailure()
    {
#pragma warning disable CS8625
        var result = LocationName.Create(null);
#pragma warning restore CS8625

        Assert.False(result.IsSuccess);
        Assert.Equal("Name is required", result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateEmptyOrWhitespaceNameReturnsFailure(string invalidName)
    {
        var result = LocationName.Create(invalidName);

        Assert.False(result.IsSuccess);
        Assert.Equal("Name is required", result.Error);
    }

    [Fact]
    public void CreateNameExceedingMaxLengthReturnsFailure()
    {
        var longName = new string('A', LocationName.MaxLength + 1);

        var result = LocationName.Create(longName);

        Assert.False(result.IsSuccess);
        Assert.Equal($"Name cannot exceed {LocationName.MaxLength} characters", result.Error);
    }

    [Fact]
    public void CreateNameWithExactMaxLengthReturnsSuccess()
    {
        var exactLengthName = new string('A', LocationName.MaxLength);

        var result = LocationName.Create(exactLengthName);

        Assert.True(result.IsSuccess);
        Assert.Equal(exactLengthName, result.Value.Value);
    }
}