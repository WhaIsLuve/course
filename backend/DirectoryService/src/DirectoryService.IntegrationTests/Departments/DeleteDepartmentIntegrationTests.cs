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
    public async Task DeleteExistingDepartmentSoftDeletesRowAndHidesIt()
    {
        var departmentId = await CreateDepartmentAsync("Department to delete", "department-delete");

        using var response = await Client.DeleteAsync(
            new Uri($"api/v1/departments/{departmentId}", UriKind.Relative));

        await response.AssertSuccessAsync();
        var exists = await QueryDatabaseAsync(db => db.Departments
            .AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == departmentId));
        Assert.True(exists);
        var department = await QueryDatabaseAsync(db => db.Departments
            .AsNoTracking()
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == departmentId));
        Assert.True(department.IsDeleted);
        Assert.NotNull(department.DeletedAt);

        using var getResponse = await Client.GetAsync(new Uri($"api/v1/departments/{departmentId}", UriKind.Relative));
        await getResponse.AssertErrorAsync(HttpStatusCode.NotFound, "department.not.found");

        using var listResponse = await Client.GetAsync(new Uri("api/v1/departments", UriKind.Relative));
        await listResponse.AssertSuccessAsync();
        using var listDocument = await listResponse.ReadJsonDocumentAsync();
        Assert.DoesNotContain(
            listDocument.RootElement.GetProperty("result").GetProperty("items").EnumerateArray(),
            item => item.GetProperty("id").GetGuid() == departmentId);
    }

    [Fact]
    public async Task DeleteMissingDepartmentReturnsNotFound()
    {
        using var response = await Client.DeleteAsync(
            new Uri($"api/v1/departments/{Guid.NewGuid()}", UriKind.Relative));

        await response.AssertErrorAsync(HttpStatusCode.NotFound, "department.not.found");
    }

    [Fact]
    public async Task DeleteDepartmentWithActiveChildrenReturnsConflict()
    {
        var parentId = await CreateDepartmentAsync("Parent department", "parent-department");
        await CreateDepartmentAsync("Child department", "child-department", parentId: parentId);

        using var response = await Client.DeleteAsync(
            new Uri($"api/v1/departments/{parentId}", UriKind.Relative));

        await response.AssertErrorAsync(HttpStatusCode.Conflict, "department.children.exist");
    }

    [Fact]
    public async Task DeletedDepartmentIsExcludedFromLocationDepartmentCount()
    {
        var locationId = await CreateLocationAsync("Location with deleted department");
        var departmentId = await CreateDepartmentAsync(
            "Department to hide from count",
            "department-to-hide-from-count",
            [locationId]);

        using var deleteResponse = await Client.DeleteAsync(
            new Uri($"api/v1/departments/{departmentId}", UriKind.Relative));
        await deleteResponse.AssertSuccessAsync();

        using var listResponse = await Client.GetAsync(
            new Uri("api/v1/locations?search=Location%20with%20deleted%20department", UriKind.Relative));
        await listResponse.AssertSuccessAsync();
        using var document = await listResponse.ReadJsonDocumentAsync();
        var item = Assert.Single(document.RootElement.GetProperty("result").GetProperty("items").EnumerateArray());
        Assert.Equal(0, item.GetProperty("departmentCount").GetInt32());
    }
}
