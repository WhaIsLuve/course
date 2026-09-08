using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class DetachPositionIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public DetachPositionIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DetachExistingPositionRemovesLink()
    {
        var positionId = await CreatePositionAsync("Detachable position");
        var departmentId = await CreateDepartmentAsync("Detachable position department", "detachable-position-department");
        using var attachResponse = await Client.PostAsync(
            new Uri($"api/v1/departments/{departmentId}/positions/{positionId}", UriKind.Relative),
            content: null);
        await attachResponse.AssertSuccessAsync();

        using var response = await Client.DeleteAsync(
            new Uri($"api/v1/departments/{departmentId}/positions/{positionId}", UriKind.Relative));

        await response.AssertSuccessAsync();
        var linkExists = await QueryDatabaseAsync(db => db.DepartmentPositions
            .AnyAsync(x => x.DepartmentId == departmentId && x.PositionId == positionId));
        Assert.False(linkExists);
    }

    [Fact]
    public async Task DetachMissingPositionLinkReturnsNotFound()
    {
        var positionId = await CreatePositionAsync("Missing detach position");
        var departmentId = await CreateDepartmentAsync("Missing position department", "missing-position-department");

        using var response = await Client.DeleteAsync(
            new Uri($"api/v1/departments/{departmentId}/positions/{positionId}", UriKind.Relative));

        await response.AssertErrorAsync(
            HttpStatusCode.NotFound,
            "department.position.not.found");
    }
}
