using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Departments;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class GetDepartmentIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public GetDepartmentIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetExistingDepartmentReturnsEnvelopeWithDepartment()
    {
        var departmentId = await CreateDepartmentAsync("Readable department", "readable-department");

        using var response = await Client.GetAsync(
            new Uri($"api/v1/departments/{departmentId}", UriKind.Relative));

        await response.AssertSuccessAsync();
        using var document = await response.ReadJsonDocumentAsync();
        Assert.Equal(
            departmentId,
            document.RootElement.GetProperty("result").GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task GetMissingDepartmentReturnsNotFoundEnvelope()
    {
        using var response = await Client.GetAsync(
            new Uri($"api/v1/departments/{Guid.NewGuid()}", UriKind.Relative));

        await response.AssertErrorAsync(HttpStatusCode.NotFound, "department.not.found");
    }
}
