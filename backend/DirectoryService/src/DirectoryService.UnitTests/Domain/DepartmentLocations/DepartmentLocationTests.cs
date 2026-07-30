using DirectoryService.Domain.DepartmentLocations;

namespace DirectoryService.UnitTests.Domain.DepartmentLocations;

public class DepartmentLocationTests
{
	private readonly DateTime _validDate = DateTime.UtcNow;

	[Fact]
	public void CreateValidDepartmentLocationReturnsSuccess()
	{
		var id = Guid.NewGuid();
		var departmentId = Guid.NewGuid();
		var locationId = Guid.NewGuid();

		var result = DepartmentLocation.Create(id, departmentId, locationId, _validDate);

		Assert.True(result.IsSuccess);
		Assert.Equal(id, result.Value.Id);
		Assert.Equal(departmentId, result.Value.DepartmentId);
		Assert.Equal(locationId, result.Value.LocationId);
		Assert.Equal(_validDate, result.Value.CreatedAt);
	}

	[Fact]
	public void CreateWithEmptyIdReturnsFailure()
	{
		var departmentId = Guid.NewGuid();
		var locationId = Guid.NewGuid();

		var result = DepartmentLocation.Create(Guid.Empty, departmentId, locationId, _validDate);

		Assert.False(result.IsSuccess);
		var errorMessages = result.Error.Messages;
		Assert.Single(errorMessages);
		Assert.Equal("Id cannot be empty", errorMessages.Single().Message);
	}

	[Fact]
	public void CreateWithEmptyDepartmentIdReturnsFailure()
	{
		var id = Guid.NewGuid();
		var locationId = Guid.NewGuid();

		var result = DepartmentLocation.Create(id, Guid.Empty, locationId, _validDate);

		Assert.False(result.IsSuccess);
		var errorMessages = result.Error.Messages;
		Assert.Single(errorMessages);
		Assert.Equal("DepartmentId cannot be empty", errorMessages.Single().Message);
	}

	[Fact]
	public void CreateWithEmptyLocationIdReturnsFailure()
	{
		var id = Guid.NewGuid();
		var departmentId = Guid.NewGuid();

		var result = DepartmentLocation.Create(id, departmentId, Guid.Empty, _validDate);

		Assert.False(result.IsSuccess);
		var errorMessages = result.Error.Messages;
		Assert.Single(errorMessages);
		Assert.Equal("LocationId cannot be empty", errorMessages.Single().Message);
	}

	[Fact]
	public void CreateWithDefaultCreatedAtReturnsFailure()
	{
		var id = Guid.NewGuid();
		var departmentId = Guid.NewGuid();
		var locationId = Guid.NewGuid();

		var result = DepartmentLocation.Create(id, departmentId, locationId, default);

		Assert.False(result.IsSuccess);
		var errorMessages = result.Error.Messages;
		Assert.Single(errorMessages);
		Assert.Equal("CreatedAt is required", errorMessages.Single().Message);
	}
}