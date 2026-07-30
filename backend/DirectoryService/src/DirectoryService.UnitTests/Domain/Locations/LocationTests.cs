using DirectoryService.Domain.Locations;

namespace DirectoryService.UnitTests.Domain.Locations;

public class LocationTests
{
    private readonly LocationName _validName = LocationName.Create("Moscow Office").Value;
    private readonly Address _validAddress = Address.Create("Russia", "Moscow", "Tverskaya", "1").Value;
    private readonly DateTime _validDate = DateTime.UtcNow;

    [Fact]
    public void CreateValidLocationReturnsSuccess()
    {
        var id = Guid.NewGuid();

        var result = Location.Create(id, _validName, _validAddress, _validDate);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(_validName, result.Value.Name);
        Assert.Equal(_validAddress, result.Value.Address);
        Assert.Equal(_validDate, result.Value.CreatedAt);
        Assert.Null(result.Value.UpdatedAt);
    }

    [Fact]
    public void CreateWithEmptyIdReturnsFailure()
    {
        var result = Location.Create(Guid.Empty, _validName, _validAddress, _validDate);

        Assert.False(result.IsSuccess);
        Assert.Equal("Id cannot be empty", result.Error);
    }

    [Fact]
    public void CreateWithDefaultCreatedAtReturnsFailure()
    {
        var result = Location.Create(Guid.NewGuid(), _validName, _validAddress, default);

        Assert.False(result.IsSuccess);
        Assert.Equal("CreatedAt is required", result.Error);
    }

    [Fact]
    public void UpdateValidDataReturnsSuccess()
    {
        var location = Location.Create(Guid.NewGuid(), _validName, _validAddress, _validDate).Value;
        var newName = LocationName.Create("New Office").Value;
        var newAddress = Address.Create("Russia", "Saint Petersburg", "Nevsky", "28").Value;
        var updatedAt = _validDate.AddHours(1);

        var result = location.Update(newName, newAddress, updatedAt);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Office", location.Name.Value);
        Assert.Equal("Saint Petersburg", location.Address.City);
        Assert.Equal(updatedAt, location.UpdatedAt);
    }

    [Fact]
    public void UpdateWithDefaultUpdatedAtReturnsFailure()
    {
        var location = Location.Create(Guid.NewGuid(), _validName, _validAddress, _validDate).Value;

        var result = location.Update(_validName, _validAddress, default);

        Assert.False(result.IsSuccess);
        Assert.Equal("UpdatedAt is required", result.Error);
    }

    [Fact]
    public void UpdateWithUpdatedAtLessThanCreatedAtReturnsFailure()
    {
        var location = Location.Create(Guid.NewGuid(), _validName, _validAddress, _validDate).Value;

        var result = location.Update(_validName, _validAddress, _validDate.AddHours(-1));

        Assert.False(result.IsSuccess);
        Assert.Equal("UpdatedAt must be greater than CreatedAt", result.Error);
    }

    [Fact]
    public void UpdateWithUpdatedAtEqualToCreatedAtReturnsFailure()
    {
        var location = Location.Create(Guid.NewGuid(), _validName, _validAddress, _validDate).Value;

        var result = location.Update(_validName, _validAddress, _validDate);

        Assert.False(result.IsSuccess);
        Assert.Equal("UpdatedAt cannot be equal to CreatedAt", result.Error);
    }
}