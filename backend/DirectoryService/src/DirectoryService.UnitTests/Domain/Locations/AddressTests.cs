using DirectoryService.Domain.Locations;

namespace DirectoryService.UnitTests.Domain.Locations;

public class AddressTests
{
    private const string ValidCountry = "Russia";
    private const string ValidCity = "Moscow";
    private const string ValidStreet = "Tverskaya";
    private const string ValidBuilding = "1";

    [Fact]
    public void CreateWithRequiredFieldsOnlyReturnsSuccess()
    {
        var result = Address.Create(ValidCountry, ValidCity, null, null);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidCountry, result.Value.Country);
        Assert.Equal(ValidCity, result.Value.City);
        Assert.Null(result.Value.Street);
        Assert.Null(result.Value.Building);
    }

    [Fact]
    public void CreateWithAllFieldsReturnsSuccess()
    {
        var result = Address.Create(ValidCountry, ValidCity, ValidStreet, ValidBuilding);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidCountry, result.Value.Country);
        Assert.Equal(ValidCity, result.Value.City);
        Assert.Equal(ValidStreet, result.Value.Street);
        Assert.Equal(ValidBuilding, result.Value.Building);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithNullOrWhitespaceCountryReturnsFailure(string? invalidCountry)
    {
#pragma warning disable CS8604
        var result = Address.Create(invalidCountry, ValidCity, null, null);
#pragma warning restore CS8604

        Assert.False(result.IsSuccess);
        Assert.Equal("Country is required", result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithNullOrWhitespaceCityReturnsFailure(string? invalidCity)
    {
#pragma warning disable CS8604
        var result = Address.Create(ValidCountry, invalidCity, null, null);
#pragma warning restore CS8604

        Assert.False(result.IsSuccess);
        Assert.Equal("City is required", result.Error);
    }

    [Fact]
    public void CreateWithCountryExceedingMaxLengthReturnsFailure()
    {
        var longCountry = new string('A', Address.CountryMaxLength + 1);

        var result = Address.Create(longCountry, ValidCity, null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal($"Country cannot exceed {Address.CountryMaxLength} characters", result.Error);
    }

    [Fact]
    public void CreateWithCityExceedingMaxLengthReturnsFailure()
    {
        var longCity = new string('A', Address.CityMaxLength + 1);

        var result = Address.Create(ValidCountry, longCity, null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal($"City cannot exceed {Address.CityMaxLength} characters", result.Error);
    }

    [Fact]
    public void CreateWithStreetExceedingMaxLengthReturnsFailure()
    {
        var longStreet = new string('A', Address.StreetMaxLength + 1);

        var result = Address.Create(ValidCountry, ValidCity, longStreet, null);

        Assert.False(result.IsSuccess);
        Assert.Equal($"Street cannot exceed {Address.StreetMaxLength} characters", result.Error);
    }

    [Fact]
    public void CreateWithBuildingExceedingMaxLengthReturnsFailure()
    {
        var longBuilding = new string('A', Address.BuildingMaxLength + 1);

        var result = Address.Create(ValidCountry, ValidCity, null, longBuilding);

        Assert.False(result.IsSuccess);
        Assert.Equal($"Building cannot exceed {Address.BuildingMaxLength} characters", result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithEmptyStreetWhenSpecifiedReturnsFailure(string emptyStreet)
    {
        var result = Address.Create(ValidCountry, ValidCity, emptyStreet, null);

        Assert.False(result.IsSuccess);
        Assert.Equal("Street cannot be empty when specified", result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithEmptyBuildingWhenSpecifiedReturnsFailure(string emptyBuilding)
    {
        var result = Address.Create(ValidCountry, ValidCity, null, emptyBuilding);

        Assert.False(result.IsSuccess);
        Assert.Equal("Building cannot be empty when specified", result.Error);
    }

    [Fact]
    public void CreateWithExactCountryMaxLengthReturnsSuccess()
    {
        var exactCountry = new string('A', Address.CountryMaxLength);

        var result = Address.Create(exactCountry, ValidCity, null, null);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CreateWithExactCityMaxLengthReturnsSuccess()
    {
        var exactCity = new string('A', Address.CityMaxLength);

        var result = Address.Create(ValidCountry, exactCity, null, null);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CreateWithExactStreetMaxLengthReturnsSuccess()
    {
        var exactStreet = new string('A', Address.StreetMaxLength);

        var result = Address.Create(ValidCountry, ValidCity, exactStreet, null);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CreateWithExactBuildingMaxLengthReturnsSuccess()
    {
        var exactBuilding = new string('A', Address.BuildingMaxLength);

        var result = Address.Create(ValidCountry, ValidCity, null, exactBuilding);

        Assert.True(result.IsSuccess);
    }
}