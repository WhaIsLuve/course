using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;

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
        Assert.False(result.Value.IsDeleted);
        Assert.Null(result.Value.DeletedAt);
    }

    [Fact]
    public void DeleteSetsDeletedState()
    {
        var location = Location.Create(Guid.NewGuid(), _validName, _validAddress, _validDate).Value;
        var deletedAt = _validDate.AddHours(1);

        var result = location.Delete(deletedAt);

        Assert.True(result.IsSuccess);
        Assert.True(location.IsDeleted);
        Assert.Equal(deletedAt, location.DeletedAt);
    }

    [Fact]
    public void DeleteBeforeCreatedAtReturnsValidation()
    {
        var location = Location.Create(Guid.NewGuid(), _validName, _validAddress, _validDate).Value;

        var result = location.Delete(_validDate.AddSeconds(-1));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void CreateWithEmptyIdReturnsFailure()
    {
        var result = Location.Create(Guid.Empty, _validName, _validAddress, _validDate);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void CreateWithDefaultCreatedAtReturnsFailure()
    {
        var result = Location.Create(Guid.NewGuid(), _validName, _validAddress, default);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
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
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void UpdateWithUpdatedAtLessThanCreatedAtReturnsFailure()
    {
        var location = Location.Create(Guid.NewGuid(), _validName, _validAddress, _validDate).Value;

        var result = location.Update(_validName, _validAddress, _validDate.AddHours(-1));

        Assert.False(result.IsSuccess);
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void UpdateWithUpdatedAtEqualToCreatedAtReturnsFailure()
    {
        var location = Location.Create(Guid.NewGuid(), _validName, _validAddress, _validDate).Value;

        var result = location.Update(_validName, _validAddress, _validDate);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Error.Messages);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
