using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Positions;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class DeletePositionIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public DeletePositionIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteUnlinkedPositionRemovesRow()
    {
        var positionId = await CreatePositionAsync("Position to delete");

        using var response = await Client.DeleteAsync(new Uri($"api/v1/positions/{positionId}", UriKind.Relative));

        await response.AssertSuccessAsync();
        var exists = await QueryDatabaseAsync(db => db.Positions.AnyAsync(x => x.Id == positionId));
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteLinkedPositionReturnsConflict()
    {
        var positionId = await CreatePositionAsync("Linked position");
        var departmentId = await CreateDepartmentAsync("Position link department", "position-link-department");
        using var attachResponse = await Client.PostAsync(
            new Uri($"api/v1/departments/{departmentId}/positions/{positionId}", UriKind.Relative),
            content: null);
        await attachResponse.AssertSuccessAsync();

        using var response = await Client.DeleteAsync(new Uri($"api/v1/positions/{positionId}", UriKind.Relative));

        await response.AssertErrorAsync(
            HttpStatusCode.Conflict,
            "position.department.links.exist");
    }

    [Fact]
    public async Task DeleteMissingPositionReturnsNotFound()
    {
        using var response = await Client.DeleteAsync(
            new Uri($"api/v1/positions/{Guid.NewGuid()}", UriKind.Relative));

        await response.AssertErrorAsync(HttpStatusCode.NotFound, "position.not.found");
    }
}
