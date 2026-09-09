using System.Net.Http.Json;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Positions;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Infrastructure;

#pragma warning disable CA1515
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly HttpClient _client;

    protected IntegrationTestBase(DirectoryServiceWebApplicationFactory factory)
    {
        Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _client = Factory.CreateClient();
    }

    protected DirectoryServiceWebApplicationFactory Factory { get; }

    protected HttpClient Client => _client;

    protected static JsonContent Json<T>(T value) => JsonContent.Create(value);

    protected async Task<Guid> CreateLocationAsync(string name = "Test location")
    {
        var dto = new CreateLocationDto(
            name,
            new AddressDto("Russia", "Moscow", "Test street", "1"));
        using var content = Json(dto);
        using var response = await Client.PostAsync(new Uri("api/v1/locations", UriKind.Relative), content);
        return await response.AssertCreatedGuidAsync();
    }

    protected async Task<Guid> CreateDepartmentAsync(
        string name = "Test department",
        string slug = "test-department",
        IReadOnlyList<Guid>? locationIds = null,
        Guid? parentId = null)
    {
        var dto = new CreateDepartmentDto(name, slug, parentId, locationIds ?? []);
        using var content = Json(dto);
        using var response = await Client.PostAsync(new Uri("api/v1/departments", UriKind.Relative), content);
        return await response.AssertCreatedGuidAsync();
    }

    protected async Task<Guid> CreatePositionAsync(string name = "Test position")
    {
        using var content = Json(new CreatePositionDto(name));
        using var response = await Client.PostAsync(new Uri("api/v1/positions", UriKind.Relative), content);
        return await response.AssertCreatedGuidAsync();
    }

    protected async Task<TResult> QueryDatabaseAsync<TResult>(Func<AppDbContext, Task<TResult>> query)
    {
        ArgumentNullException.ThrowIfNull(query);
        await using var scope = Factory.Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    public Task InitializeAsync() => Factory.ResetDatabaseAsync();

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }
}
#pragma warning restore CA1515
