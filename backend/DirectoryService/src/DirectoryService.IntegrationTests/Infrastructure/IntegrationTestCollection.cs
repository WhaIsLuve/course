namespace DirectoryService.IntegrationTests.Infrastructure;

internal static class DirectoryServiceIntegrationTestGroup
{
    public const string Name = "Directory Service integration tests";
}

[CollectionDefinition(DirectoryServiceIntegrationTestGroup.Name, DisableParallelization = true)]
#pragma warning disable CA1515
public sealed class DirectoryServiceIntegrationTestGroupDefinition
{
}
#pragma warning restore CA1515
