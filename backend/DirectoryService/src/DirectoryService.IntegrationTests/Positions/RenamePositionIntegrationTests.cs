using System.Net;
using DirectoryService.Contracts.Positions;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Positions;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class RenamePositionIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public RenamePositionIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task RenameExistingPositionPersistsNewName()
    {
        var positionId = await CreatePositionAsync("Position before rename");
        using var content = Json(new UpdatePositionDto("Position after rename"));

        using var response = await Client.PatchAsync(
            new Uri($"api/v1/positions/{positionId}", UriKind.Relative), content);

        await response.AssertSuccessAsync();
        var position = await QueryDatabaseAsync(db => db.Positions
            .AsNoTracking()
            .SingleAsync(x => x.Id == positionId));
        Assert.Equal("Position after rename", position.Name.Value);
    }

    [Fact]
    public async Task RenameMissingPositionReturnsNotFound()
    {
        using var content = Json(new UpdatePositionDto("Missing position"));

        using var response = await Client.PatchAsync(
            new Uri($"api/v1/positions/{Guid.NewGuid()}", UriKind.Relative), content);

        await response.AssertErrorAsync(HttpStatusCode.NotFound, "position.not.found");
    }
}
