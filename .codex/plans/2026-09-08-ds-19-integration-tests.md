# DS-19: локальные HTTP-интеграционные тесты Directory Service

Этот план фиксирует реализацию инфраструктуры и покрытия integration-тестами.

## Purpose / Big Picture

Разработчик сможет запускать отдельный тестовый проект, который поднимает
настоящий ASP.NET Core pipeline и временный PostgreSQL, проверяет Envelope и
состояние БД, а затем очищает данные для следующего сценария.

## Progress

- [x] (2026-09-08) Добавлены package versions, integration project и проект в solution.
- [x] (2026-09-08) Добавлены Program entry point, class-scoped
  Testcontainers/Postgres fixture, EF migrations и Respawn reset.
- [x] (2026-09-08) Добавлены Envelope/HTTP helpers и три ручных эталонных теста.
- [x] (2026-09-08) Сгенерировано и проревьюено feature-specific покрытие
  DS-5—DS-18: отдельный class на каждую реализованную операцию.
- [x] (2026-09-08) Выполнен runtime integration suite на доступном Docker daemon;
  первый и повторный прогоны завершились 37/37.
- [x] (2026-09-08) Выполнены unit suite и финальная сборка solution.

## Surprises & Discoveries

- Restore потребовал поднять централизованные версии
  `Microsoft.Extensions.Logging.Console` и `Microsoft.Extensions.Logging.Abstractions`
  с 10.0.7 до 10.0.9 из-за зависимостей `Microsoft.AspNetCore.Mvc.Testing`.
- Первый запуск в sandbox не имел доступа к
  `npipe://./pipe/docker_engine`; после запуска с правами Docker Desktop suite
  выполнен успешно. Тестовые классы объединены в непараллельную xUnit-коллекцию,
  чтобы отдельные WebApplicationFactory и контейнеры не стартовали одновременно.

## Decision Log

- Каждый operation-specific integration class объявляет
  `IClassFixture<DirectoryServiceWebApplicationFactory>` и получает собственный
  PostgreSQL-контейнер; Respawn очищает прикладные таблицы перед каждым тестом
  класса, а `__EFMigrationsHistory` сохраняется.
- Текущие заглушки Position GET не покрываются; тестируются только реализованные
  операции Position create/rename/delete.

## Outcomes & Retrospective

Проект собирается без warnings/errors, 37 integration-сценариев обнаруживаются
test runner-ом, два последовательных runtime-прогона завершились успешно, а
unit suite из 163 тестов проходит. Production repositories и DbContext не
заменяются.

## Context and Orientation

Основное приложение находится в `backend/DirectoryService/src/DirectoryService.Web`.
`AppDbContext` и миграции находятся в `DirectoryService.Infrastructure.Postgres`.
Новый проект `DirectoryService.IntegrationTests` использует `WebApplicationFactory<Program>`
и прямую проверку реального `AppDbContext` после HTTP-запросов.

## Plan of Work

Fixture подменяет только connection string через in-memory configuration,
запускает миграции и создаёт Respawn checkpoint. Каждый feature-specific class
переиспользует только инфраструктурный base/helper, но владеет своим fixture.
Test helpers проверяют status, JSON Envelope, error code/type и timestamp.
Ручные тесты служат reference для AI-генерации остальных сценариев; сгенерированный
код проверен на HTTP-only setup, отсутствие моков и DB assertions для write-операций.

## Concrete Steps

Из корня репозитория:

    dotnet restore backend/DirectoryService/src/DirectoryService.IntegrationTests/DirectoryService.IntegrationTests.csproj
    dotnet build backend/DirectoryService/src/DirectoryService.IntegrationTests/DirectoryService.IntegrationTests.csproj --no-restore
    dotnet test backend/DirectoryService/src/DirectoryService.IntegrationTests/DirectoryService.IntegrationTests.csproj

Перед последней командой должен быть доступен `docker version`.

## Validation and Acceptance

- Integration project и solution собираются с нулём предупреждений и ошибок.
- Каждый тест ходит в `/api/v1/...` через `HttpClient`.
- Write-тесты проверяют persisted/unchanged/deleted rows и связи.
- Повторный запуск suite проходит на чистом состоянии и не зависит от порядка.
- Unit suite остаётся зелёным.

## Idempotence and Recovery

Контейнер и его данные удаляются Testcontainers после fixture. Respawn безопасен
для повторных запусков; миграции применяются к новой базе. При недоступном Docker
остаются доступными restore/build, а после запуска Docker Desktop suite запускается
той же отдельной командой без изменения тестовой схемы.

## Interfaces and Dependencies

- `DirectoryServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime`.
- `IntegrationTestBase` выполняет reset перед каждым test instance.
- NuGet: `Microsoft.AspNetCore.Mvc.Testing` 10.0.9,
  `Testcontainers.PostgreSql` 4.15.0, `Respawn` 7.0.0.
