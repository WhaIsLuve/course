using System.Net;
using DirectoryService.Contracts.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class CreateDepartmentIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public CreateDepartmentIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateValidDepartmentPersistsDepartment()
    {
        var departmentId = await CreateDepartmentAsync("Created department", "created-department");
        var department = await QueryDatabaseAsync(db => db.Departments
            .AsNoTracking()
            .SingleAsync(x => x.Id == departmentId));

        Assert.Equal("Created department", department.Name.Value);
        Assert.Equal("created-department", department.Slug.Value);
    }

    [Fact]
    public async Task CreateDepartmentWithMissingLocationReturnsNotFoundAndDoesNotPersist()
    {
        var missingLocationId = Guid.NewGuid();
        var dto = new CreateDepartmentDto(
            "Missing location department",
            "missing-location-department",
            null,
            [missingLocationId]);
        using var content = Json(dto);

        using var response = await Client.PostAsync(new Uri("api/v1/departments", UriKind.Relative), content);

        await response.AssertErrorAsync(HttpStatusCode.NotFound, "location.missing");
        var departmentExists = await QueryDatabaseAsync(db => db.Departments
            .AsNoTracking()
            .AnyAsync(x => x.Slug.Value == dto.Slug));
        Assert.False(departmentExists);
    }
}
