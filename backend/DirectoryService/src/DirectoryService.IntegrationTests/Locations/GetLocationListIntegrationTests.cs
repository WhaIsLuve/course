using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Locations;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class GetLocationListIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public GetLocationListIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetLocationsFiltersAndPaginatesEnvelope()
    {
        await CreateLocationAsync("List location alpha");
        await CreateLocationAsync("List location beta");

        using var response = await Client.GetAsync(new Uri(
            "api/v1/locations?search=List%20location&page=1&pageSize=1",
            UriKind.Relative));

        await response.AssertSuccessAsync();
        using var document = await response.ReadJsonDocumentAsync();
        var result = document.RootElement.GetProperty("result");
        Assert.Equal(2, result.GetProperty("totalCount").GetInt32());
        Assert.Single(result.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task GetLocationsWithInvalidPageSizeReturnsValidation()
    {
        using var response = await Client.GetAsync(new Uri(
            "api/v1/locations?pageSize=101",
            UriKind.Relative));

        await response.AssertErrorAsync(
            HttpStatusCode.BadRequest,
            "locations.page.size.invalid");
    }
}
