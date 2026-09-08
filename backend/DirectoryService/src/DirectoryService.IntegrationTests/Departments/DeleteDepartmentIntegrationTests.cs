using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class DeleteDepartmentIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public DeleteDepartmentIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteExistingDepartmentRemovesRow()
    {
        var departmentId = await CreateDepartmentAsync("Department to delete", "department-delete");

        using var response = await Client.DeleteAsync(
            new Uri($"api/v1/departments/{departmentId}", UriKind.Relative));

        await response.AssertSuccessAsync();
        var exists = await QueryDatabaseAsync(db => db.Departments
            .AsNoTracking()
            .AnyAsync(x => x.Id == departmentId));
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteMissingDepartmentReturnsNotFound()
    {
        using var response = await Client.DeleteAsync(
            new Uri($"api/v1/departments/{Guid.NewGuid()}", UriKind.Relative));

        await response.AssertErrorAsync(HttpStatusCode.NotFound, "department.not.found");
    }
}
