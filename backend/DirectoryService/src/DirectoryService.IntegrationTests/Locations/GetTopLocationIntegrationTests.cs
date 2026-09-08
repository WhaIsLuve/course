using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Locations;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class GetTopLocationIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public GetTopLocationIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetTopLocationsReturnsAtMostFiveRows()
    {
        for (var index = 0; index < 6; index++)
            await CreateLocationAsync($"Top location {index}");

        using var response = await Client.GetAsync(new Uri("api/v1/locations/top", UriKind.Relative));

        await response.AssertSuccessAsync();
        using var document = await response.ReadJsonDocumentAsync();
        Assert.Equal(5, document.RootElement.GetProperty("result").GetArrayLength());
    }
}
