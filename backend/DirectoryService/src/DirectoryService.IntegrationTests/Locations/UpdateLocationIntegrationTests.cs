using System.Net;
using DirectoryService.Contracts.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Locations;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class UpdateLocationIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public UpdateLocationIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task UpdateExistingLocationPersistsNewValues()
    {
        var locationId = await CreateLocationAsync("Location before update");
        var dto = new UpdateLocationDto(
            "Location after update",
            new AddressDto("Russia", "Kazan", "Baumana", "2"));
        using var content = Json(dto);

        using var response = await Client.PatchAsync(
            new Uri($"api/v1/locations/{locationId}", UriKind.Relative), content);

        await response.AssertSuccessAsync();
        var location = await QueryDatabaseAsync(db => db.Locations
            .AsNoTracking()
            .SingleAsync(x => x.Id == locationId));
        Assert.Equal(dto.Name, location.Name.Value);
        Assert.Equal(dto.Address.City, location.Address.City);
    }

    [Fact]
    public async Task UpdateMissingLocationReturnsNotFound()
    {
        using var content = Json(new UpdateLocationDto(
            "Missing location",
            new AddressDto("Russia", "Moscow", "Street", "1")));

        using var response = await Client.PatchAsync(
            new Uri($"api/v1/locations/{Guid.NewGuid()}", UriKind.Relative), content);

        await response.AssertErrorAsync(HttpStatusCode.NotFound, "location.not.found");
    }
}
