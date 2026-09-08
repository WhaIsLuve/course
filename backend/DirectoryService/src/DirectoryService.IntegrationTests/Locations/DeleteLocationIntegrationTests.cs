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
    public async Task DeleteUnlinkedLocationRemovesRow()
    {
        var locationId = await CreateLocationAsync("Location to delete");

        using var response = await Client.DeleteAsync(new Uri($"api/v1/locations/{locationId}", UriKind.Relative));

        await response.AssertSuccessAsync();
        var exists = await QueryDatabaseAsync(db => db.Locations
            .AsNoTracking()
            .AnyAsync(x => x.Id == locationId));
        Assert.False(exists);
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
}
