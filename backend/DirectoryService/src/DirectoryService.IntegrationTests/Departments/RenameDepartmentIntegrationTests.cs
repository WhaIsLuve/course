using System.Net;
using DirectoryService.Contracts.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class RenameDepartmentIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public RenameDepartmentIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task RenameExistingDepartmentPersistsNewName()
    {
        var departmentId = await CreateDepartmentAsync("Department before rename", "department-rename");
        using var content = Json(new UpdateDepartmentNameDto("Department after rename"));

        using var response = await Client.PatchAsync(
            new Uri($"api/v1/departments/{departmentId}", UriKind.Relative), content);

        await response.AssertSuccessAsync();
        var department = await QueryDatabaseAsync(db => db.Departments
            .AsNoTracking()
            .SingleAsync(x => x.Id == departmentId));
        Assert.Equal("Department after rename", department.Name.Value);
    }

    [Fact]
    public async Task RenameMissingDepartmentReturnsNotFound()
    {
        using var content = Json(new UpdateDepartmentNameDto("Missing department"));

        using var response = await Client.PatchAsync(
            new Uri($"api/v1/departments/{Guid.NewGuid()}", UriKind.Relative), content);

        await response.AssertErrorAsync(HttpStatusCode.NotFound, "department.not.found");
    }
}
