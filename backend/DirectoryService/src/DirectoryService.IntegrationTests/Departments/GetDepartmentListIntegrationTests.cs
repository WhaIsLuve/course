using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Departments;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class GetDepartmentListIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public GetDepartmentListIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetDepartmentsFiltersAndPaginatesEnvelope()
    {
        await CreateDepartmentAsync("List department alpha", "list-department-alpha");
        await CreateDepartmentAsync("List department beta", "list-department-beta");

        using var response = await Client.GetAsync(new Uri(
            "api/v1/departments?search=List%20department&page=1&pageSize=1",
            UriKind.Relative));

        await response.AssertSuccessAsync();
        using var document = await response.ReadJsonDocumentAsync();
        var result = document.RootElement.GetProperty("result");
        Assert.Equal(2, result.GetProperty("totalCount").GetInt32());
        Assert.Single(result.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task GetDepartmentsWithInvalidPageReturnsValidation()
    {
        using var response = await Client.GetAsync(new Uri(
            "api/v1/departments?page=0",
            UriKind.Relative));

        await response.AssertErrorAsync(
            HttpStatusCode.BadRequest,
            "departments.page.invalid");
    }
}
