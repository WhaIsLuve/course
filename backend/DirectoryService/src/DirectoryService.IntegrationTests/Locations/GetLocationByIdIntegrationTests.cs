using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Locations;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class GetLocationByIdIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public GetLocationByIdIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetExistingLocationReturnsEnvelopeWithLocation()
    {
        var locationId = await CreateLocationAsync("Readable location");

        using var response = await Client.GetAsync(new Uri($"api/v1/locations/{locationId}", UriKind.Relative));

        await response.AssertSuccessAsync();
        using var document = await response.ReadJsonDocumentAsync();
        var result = document.RootElement.GetProperty("result");
        Assert.Equal(locationId, result.GetProperty("id").GetGuid());
        Assert.Equal("Readable location", result.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetMissingLocationReturnsNotFoundEnvelope()
    {
        using var response = await Client.GetAsync(
            new Uri($"api/v1/locations/{Guid.NewGuid()}", UriKind.Relative));

        await response.AssertErrorAsync(HttpStatusCode.NotFound, "location.not.found");
    }
}
