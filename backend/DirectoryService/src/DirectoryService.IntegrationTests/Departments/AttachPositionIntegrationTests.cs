using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class AttachPositionIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public AttachPositionIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AttachExistingPositionCreatesLink()
    {
        var positionId = await CreatePositionAsync("Attachable position");
        var departmentId = await CreateDepartmentAsync("Position department", "position-department");

        using var response = await Client.PostAsync(
            new Uri($"api/v1/departments/{departmentId}/positions/{positionId}", UriKind.Relative),
            content: null);

        await response.AssertSuccessAsync();
        var linkExists = await QueryDatabaseAsync(db => db.DepartmentPositions
            .AnyAsync(x => x.DepartmentId == departmentId && x.PositionId == positionId));
        Assert.True(linkExists);
    }

    [Fact]
    public async Task AttachDuplicatePositionReturnsConflict()
    {
        var positionId = await CreatePositionAsync("Duplicate attach position");
        var departmentId = await CreateDepartmentAsync("Duplicate position department", "duplicate-position-department");
        using var firstResponse = await Client.PostAsync(
            new Uri($"api/v1/departments/{departmentId}/positions/{positionId}", UriKind.Relative),
            content: null);
        await firstResponse.AssertSuccessAsync();

        using var response = await Client.PostAsync(
            new Uri($"api/v1/departments/{departmentId}/positions/{positionId}", UriKind.Relative),
            content: null);

        await response.AssertErrorAsync(HttpStatusCode.Conflict, "department.position.exists");
    }
}
