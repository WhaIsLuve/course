using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Positions;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class GetPositionIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public GetPositionIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetExistingPositionReturnsEnvelopeWithPosition()
    {
        var positionId = await CreatePositionAsync("Readable position");

        using var response = await Client.GetAsync(
            new Uri($"api/v1/positions/{positionId}", UriKind.Relative));

        await response.AssertSuccessAsync();
        using var document = await response.ReadJsonDocumentAsync();
        var result = document.RootElement.GetProperty("result");
        Assert.Equal(positionId, result.GetProperty("id").GetGuid());
        Assert.Equal("Readable position", result.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetPositionsFiltersAndPaginatesEnvelope()
    {
        await CreatePositionAsync("List position alpha");
        await CreatePositionAsync("List position beta");

        using var response = await Client.GetAsync(new Uri(
            "api/v1/positions?search=List%20position&page=1&pageSize=1",
            UriKind.Relative));

        await response.AssertSuccessAsync();
        using var document = await response.ReadJsonDocumentAsync();
        var result = document.RootElement.GetProperty("result");
        Assert.Equal(2, result.GetProperty("totalCount").GetInt32());
        Assert.Single(result.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task GetMissingPositionReturnsNotFoundEnvelope()
    {
        using var response = await Client.GetAsync(
            new Uri($"api/v1/positions/{Guid.NewGuid()}", UriKind.Relative));

        await response.AssertErrorAsync(HttpStatusCode.NotFound, "position.not.found");
    }
}
