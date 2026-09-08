using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class DetachLocationIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public DetachLocationIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DetachExistingLocationRemovesLink()
    {
        var locationId = await CreateLocationAsync("Detachable location");
        var departmentId = await CreateDepartmentAsync("Detachable department", "detachable-department", [locationId]);

        using var response = await Client.DeleteAsync(
            new Uri($"api/v1/departments/{departmentId}/location/{locationId}", UriKind.Relative));

        await response.AssertSuccessAsync();
        var linkExists = await QueryDatabaseAsync(db => db.DepartmentLocations
            .AnyAsync(x => x.DepartmentId == departmentId && x.LocationId == locationId));
        Assert.False(linkExists);
    }

    [Fact]
    public async Task DetachMissingLinkReturnsNotFound()
    {
        var locationId = await CreateLocationAsync("Missing detach location");
        var departmentId = await CreateDepartmentAsync("Missing detach department", "missing-detach-department");

        using var response = await Client.DeleteAsync(
            new Uri($"api/v1/departments/{departmentId}/location/{locationId}", UriKind.Relative));

        await response.AssertErrorAsync(
            HttpStatusCode.NotFound,
            "department.location.not.found");
    }
}
