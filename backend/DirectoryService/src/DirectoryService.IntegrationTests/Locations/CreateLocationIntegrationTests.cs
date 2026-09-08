using System.Net;
using DirectoryService.Contracts.Locations;
using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Locations;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class CreateLocationIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public CreateLocationIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateValidLocationReturnsCreatedEnvelopeAndPersistsRow()
    {
        var dto = new CreateLocationDto(
            "Manual headquarters",
            new AddressDto("Russia", "Moscow", "Tverskaya", "1"));

        using var content = Json(dto);
        using var response = await Client.PostAsync(new Uri("api/v1/locations", UriKind.Relative), content);

        var locationId = await response.AssertCreatedGuidAsync();
        var location = await QueryDatabaseAsync(db => db.Locations.FindAsync([locationId]).AsTask());

        Assert.NotNull(location);
        Assert.Equal(dto.Name, location.Name.Value);
        Assert.Equal(dto.Address.Country, location.Address.Country);
        Assert.Equal(dto.Address.City, location.Address.City);
        Assert.Equal(dto.Address.Street, location.Address.Street);
        Assert.Equal(dto.Address.Building, location.Address.Building);
    }

    [Fact]
    public async Task CreateLocationWithDuplicateNameReturnsConflictEnvelope()
    {
        await CreateLocationAsync("Duplicate location");
        using var content = Json(new CreateLocationDto(
            "Duplicate location",
            new AddressDto("Russia", "Moscow", "Street", "2")));

        using var response = await Client.PostAsync(new Uri("api/v1/locations", UriKind.Relative), content);

        await response.AssertErrorAsync(HttpStatusCode.Conflict, "location.name.exists");
    }
}
