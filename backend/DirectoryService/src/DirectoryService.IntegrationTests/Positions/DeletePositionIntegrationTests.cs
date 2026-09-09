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
    public async Task DeleteUnlinkedPositionSoftDeletesRowAndHidesIt()
    {
        var positionId = await CreatePositionAsync("Position to delete");

        using var response = await Client.DeleteAsync(new Uri($"api/v1/positions/{positionId}", UriKind.Relative));

        await response.AssertSuccessAsync();
        var exists = await QueryDatabaseAsync(db => db.Positions
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == positionId));
        Assert.True(exists);
        var position = await QueryDatabaseAsync(db => db.Positions
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == positionId));
        Assert.True(position.IsDeleted);
        Assert.NotNull(position.DeletedAt);

        using var getResponse = await Client.GetAsync(new Uri($"api/v1/positions/{positionId}", UriKind.Relative));
        await getResponse.AssertErrorAsync(HttpStatusCode.NotFound, "position.not.found");

        using var listResponse = await Client.GetAsync(new Uri("api/v1/positions", UriKind.Relative));
        await listResponse.AssertSuccessAsync();
        using var listDocument = await listResponse.ReadJsonDocumentAsync();
        Assert.DoesNotContain(
            listDocument.RootElement.GetProperty("result").GetProperty("items").EnumerateArray(),
            item => item.GetProperty("id").GetGuid() == positionId);
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

    [Fact]
    public async Task DeletedPositionNameCanBeReused()
    {
        var positionId = await CreatePositionAsync("Reusable position");

        using var deleteResponse = await Client.DeleteAsync(new Uri($"api/v1/positions/{positionId}", UriKind.Relative));
        await deleteResponse.AssertSuccessAsync();

        var replacementId = await CreatePositionAsync("Reusable position");

        Assert.NotEqual(positionId, replacementId);
    }
}
