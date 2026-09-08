хъ# Directory Service

## Integration tests

Integration tests run the real Web API against a temporary PostgreSQL 16
container. Start Docker Desktop and verify `docker version`, then run only this
suite from the repository root:

    dotnet test backend/DirectoryService/src/DirectoryService.IntegrationTests/DirectoryService.IntegrationTests.csproj

Each feature-specific test class owns its Testcontainers fixture. EF migrations
are applied when the class fixture starts, and Respawn clears application tables
before each test.
