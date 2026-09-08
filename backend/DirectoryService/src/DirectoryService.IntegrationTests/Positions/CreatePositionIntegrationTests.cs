using System.Net;
using DirectoryService.Contracts.Positions;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Positions;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class CreatePositionIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public CreatePositionIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateValidPositionReturnsCreatedEnvelope()
    {
        var positionId = await CreatePositionAsync("Created position");
        var exists = await QueryDatabaseAsync(db => db.Positions.AnyAsync(x => x.Id == positionId));

        Assert.True(exists);
    }

    [Fact]
    public async Task CreatePositionWithDuplicateNameReturnsConflict()
    {
        await CreatePositionAsync("Duplicate position");
        using var content = Json(new CreatePositionDto("Duplicate position"));

        using var response = await Client.PostAsync(new Uri("api/v1/positions", UriKind.Relative), content);

        await response.AssertErrorAsync(HttpStatusCode.Conflict, "position.name.exists");
    }

    [Fact]
    public async Task CreatePositionWithEmptyNameReturnsValidation()
    {
        using var content = Json(new CreatePositionDto(string.Empty));

        using var response = await Client.PostAsync(new Uri("api/v1/positions", UriKind.Relative), content);

        await response.AssertErrorAsync(HttpStatusCode.BadRequest, "position.name.required");
    }
}
