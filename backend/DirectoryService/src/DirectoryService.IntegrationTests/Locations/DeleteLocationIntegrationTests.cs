using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Locations;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class DeleteLocationIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public DeleteLocationIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteUnlinkedLocationSoftDeletesRowAndHidesIt()
    {
        var locationId = await CreateLocationAsync("Location to delete");

        using var response = await Client.DeleteAsync(new Uri($"api/v1/locations/{locationId}", UriKind.Relative));

        await response.AssertSuccessAsync();
        var exists = await QueryDatabaseAsync(db => db.Locations
            .AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == locationId));
        Assert.True(exists);
        var location = await QueryDatabaseAsync(db => db.Locations
            .AsNoTracking()
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == locationId));
        Assert.True(location.IsDeleted);
        Assert.NotNull(location.DeletedAt);

        using var getResponse = await Client.GetAsync(new Uri($"api/v1/locations/{locationId}", UriKind.Relative));
        await getResponse.AssertErrorAsync(HttpStatusCode.NotFound, "location.not.found");

        using var listResponse = await Client.GetAsync(new Uri("api/v1/locations", UriKind.Relative));
        await listResponse.AssertSuccessAsync();
        using var listDocument = await listResponse.ReadJsonDocumentAsync();
        Assert.DoesNotContain(
            listDocument.RootElement.GetProperty("result").GetProperty("items").EnumerateArray(),
            item => item.GetProperty("id").GetGuid() == locationId);

        using var topResponse = await Client.GetAsync(new Uri("api/v1/locations/top", UriKind.Relative));
        await topResponse.AssertSuccessAsync();
        using var topDocument = await topResponse.ReadJsonDocumentAsync();
        Assert.DoesNotContain(
            topDocument.RootElement.GetProperty("result").EnumerateArray(),
            item => item.GetProperty("id").GetGuid() == locationId);
    }

    [Fact]
    public async Task DeleteLinkedLocationReturnsConflict()
    {
        var locationId = await CreateLocationAsync("Linked location");
        await CreateDepartmentAsync("Linked department", "linked-department", [locationId]);

        using var response = await Client.DeleteAsync(new Uri($"api/v1/locations/{locationId}", UriKind.Relative));

        await response.AssertErrorAsync(
            HttpStatusCode.Conflict,
            "location.department.links.exist");
    }

    [Fact]
    public async Task DeletedLocationNameCanBeReused()
    {
        var locationId = await CreateLocationAsync("Reusable location");

        using var deleteResponse = await Client.DeleteAsync(new Uri($"api/v1/locations/{locationId}", UriKind.Relative));
        await deleteResponse.AssertSuccessAsync();

        var replacementId = await CreateLocationAsync("Reusable location");

        Assert.NotEqual(locationId, replacementId);
    }
}
