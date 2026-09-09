using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure.Postgres;
using DirectoryService.Infrastructure.Postgres.BackgroundCleanup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Infrastructure;

[Collection(DirectoryServiceIntegrationTestGroup.Name)]
public sealed class SoftDeleteCleanupIntegrationTests
    : IntegrationTestBase, IClassFixture<DirectoryServiceWebApplicationFactory>
{
    public SoftDeleteCleanupIntegrationTests(DirectoryServiceWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task PurgeRemovesExpiredRowsInBatchesAndKeepsRecentRows()
    {
        var now = DateTime.UtcNow;
        var oldCreatedAt = now.AddDays(-40);
        var oldDeletedAt = now.AddDays(-31);
        var recentCreatedAt = now.AddDays(-2);
        var recentDeletedAt = now.AddDays(-1);

        var oldLocations = Enumerable.Range(1, 3)
            .Select(index => BuildExpiredLocation(Guid.CreateVersion7(), $"Expired location {index}", oldCreatedAt, oldDeletedAt))
            .ToArray();
        var oldDepartment = BuildExpiredDepartment(Guid.CreateVersion7(), oldCreatedAt, oldDeletedAt);
        var oldPosition = BuildExpiredPosition(Guid.CreateVersion7(), oldCreatedAt, oldDeletedAt);
        var recentLocation = BuildExpiredLocation(Guid.CreateVersion7(), "Recent location", recentCreatedAt, recentDeletedAt);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.AddRange(oldLocations);
            db.AddRange(oldDepartment, oldPosition, recentLocation);
            await db.SaveChangesAsync();

            var purger = scope.ServiceProvider.GetRequiredService<ISoftDeletedEntitiesPurger>();
            var deletedCount = await purger.PurgeAsync();

            Assert.Equal(5, deletedCount);
        }

        var remainingRecent = await QueryDatabaseAsync(db => db.Locations
            .IgnoreQueryFilters()
            .AnyAsync(location => location.Id == recentLocation.Id));
        Assert.True(remainingRecent);

        var remainingOld = await QueryDatabaseAsync(db => db.Locations
            .IgnoreQueryFilters()
            .AnyAsync(location => oldLocations.Select(x => x.Id).Contains(location.Id)));
        Assert.False(remainingOld);
        Assert.False(await QueryDatabaseAsync(db => db.Departments
            .IgnoreQueryFilters()
            .AnyAsync(department => department.Id == oldDepartment.Id)));
        Assert.False(await QueryDatabaseAsync(db => db.Positions
            .IgnoreQueryFilters()
            .AnyAsync(position => position.Id == oldPosition.Id)));
    }

    private static Location BuildExpiredLocation(Guid id, string name, DateTime createdAt, DateTime deletedAt)
    {
        var location = Location.Create(
            id,
            LocationName.Create(name).Value,
            Address.Create("Russia", "Moscow", null, null).Value,
            createdAt).Value;
        Assert.True(location.Delete(deletedAt).IsSuccess);
        return location;
    }

    private static Department BuildExpiredDepartment(Guid id, DateTime createdAt, DateTime deletedAt)
    {
        var department = Department.Create(
            id,
            DepartmentName.Create("Expired department").Value,
            DepartmentSlug.Create($"expired-{id:N}").Value,
            null,
            createdAt).Value;
        Assert.True(department.Delete(deletedAt).IsSuccess);
        return department;
    }

    private static Position BuildExpiredPosition(Guid id, DateTime createdAt, DateTime deletedAt)
    {
        var position = Position.Create(
            id,
            PositionName.Create($"Expired position {id:N}").Value,
            createdAt).Value;
        Assert.True(position.Delete(deletedAt).IsSuccess);
        return position;
    }
}
