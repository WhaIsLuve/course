using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class AttachLocationIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public AttachLocationIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AttachExistingLocationCreatesLink()
    {
        var locationId = await CreateLocationAsync("Attachable location");
        var departmentId = await CreateDepartmentAsync("Attachable department", "attachable-department");

        using var response = await Client.PostAsync(
            new Uri($"api/v1/departments/{departmentId}/location/{locationId}", UriKind.Relative),
            content: null);

        await response.AssertSuccessAsync();
        var linkExists = await QueryDatabaseAsync(db => db.DepartmentLocations
            .AnyAsync(x => x.DepartmentId == departmentId && x.LocationId == locationId));
        Assert.True(linkExists);
    }

    [Fact]
    public async Task AttachDuplicateLocationReturnsConflict()
    {
        var locationId = await CreateLocationAsync("Duplicate attach location");
        var departmentId = await CreateDepartmentAsync("Duplicate attach department", "duplicate-attach-department");
        using var firstResponse = await Client.PostAsync(
            new Uri($"api/v1/departments/{departmentId}/location/{locationId}", UriKind.Relative),
            content: null);
        await firstResponse.AssertSuccessAsync();

        using var response = await Client.PostAsync(
            new Uri($"api/v1/departments/{departmentId}/location/{locationId}", UriKind.Relative),
            content: null);

        await response.AssertErrorAsync(HttpStatusCode.Conflict, "department.location.exist");
    }
}
