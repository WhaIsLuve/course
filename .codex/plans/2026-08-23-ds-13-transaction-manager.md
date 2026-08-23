# DS-13: TransactionManager и декораторы команд

Этот план фиксирует реализацию DS-13 и поддерживается по правилам `PLANS.MD`.

## Цель

Собрать в одной scoped-инфраструктуре сохранение, явные транзакции и безопасное
преобразование ошибок EF Core/PostgreSQL, оставив handler-ы бизнес-слоем.

## Progress

- [x] (2026-08-23) Создана ветка `feature/DS-13`.
- [x] (2026-08-23) Исследованы текущие handler-ы, репозитории, DI и Result-поток.
- [x] (2026-08-23) Добавить pipeline-контракты и Scrutor-декораторы.
- [x] (2026-08-23) Реализовать scoped TransactionManager и DB error mapping.
- [x] (2026-08-23) Убрать сохранение из DbContext, репозиториев и handler-ов.
- [x] (2026-08-23) Перенести validators на команды и обновить DI.
- [x] (2026-08-23) Выполнить build, существующие tests и статический поиск.

## Surprises & Discoveries

- Инфраструктура уже содержит Scrutor и scoped `AddDbContext`, но собственных
  транзакций нет; `Save` делегирован из двух репозиториев в `AppDbContext`.
- Уникальный индекс в задачу не входит по подтверждённому решению, поэтому
  mapping SQLSTATE 23505 реализуется без миграции.

## Decision Log

- `CreateDepartmentCommand` использует явную транзакцию; остальные текущие
  write-команды используют единый commit `SaveChangesAsync`.
- Валидация выполняется generic-декоратором; validators принимают команды, а не
  DTO. HTTP-контракт не меняется.
- Новые DS-13 integration/unit tests не добавляются; rollback и SQLSTATE будут
  покрыты отдельной задачей.

## Outcomes & Retrospective

Реализованы единая граница сохранения, явная транзакция для создания
подразделения со связями, декораторная валидация/логирование и безопасный
SQLSTATE mapping. Реальная PostgreSQL-проверка rollback отложена в отдельную
задачу согласно согласованному Test Plan.

## Validation

- `dotnet build` для всех проектов Directory Service.
- Существующие `dotnet test`.
- `rg` не должен находить `SaveChanges`, `Save` или DB exception handling в
  handler-ах и репозиториях.

## Context and Orientation

Directory Service разделён на Core, Infrastructure.Postgres и Web. Команды и
handler-ы находятся в `Core.Features`, репозитории используют общий scoped
`AppDbContext`, а контроллеры получают `ICommandHandler<,>` из DI. Scrutor уже
подключён в Core; PostgreSQL и EF Core уже подключены в Infrastructure.

## Plan of Work

Pipeline регистрирует сначала transaction decorator, затем validation и затем
logging, поэтому logging является внешним слоем, validation выполняется до
начала транзакции, а commit выполняется после handler-а. `TransactionManager`
реализует сохранение, явную транзакцию и SQLSTATE mapping. Репозитории только
меняют tracking state контекста. Validators принимают команды и возвращают
ошибки через общий `CreateFailure` контракт команды.

## Concrete Steps

Рабочая директория: `C:\Users\vlad\Projects\sachkovCource\course`.

    git switch feature/DS-13
    dotnet build backend/DirectoryService/src/DirectoryService.Web/DirectoryService.Web.csproj --no-restore
    dotnet test backend/DirectoryService/src/DirectoryService.UnitTests/DirectoryService.UnitTests.csproj --no-restore
    rg -n --glob "*.cs" "SaveChanges|\\.Save\\(|Save\\(" backend/DirectoryService/src/DirectoryService.Core/Features backend/DirectoryService/src/DirectoryService.Infrastructure.Postgres/Repositories

Ожидаемый результат последних двух команд: успешная сборка, 139 passed tests и
отсутствие совпадений в handler-ах/репозиториях.

## Validation and Acceptance

Обычная команда после успешного handler-а вызывает один `SaveChangesAsync`.
Транзакционная команда начинает EF-транзакцию до handler-а, делает save и
commit после успеха; failure, DB exception или отмена запроса выполняют rollback.
SQLSTATE 23505, 23503, concurrency conflict и остальные DB failures получают
разные безопасные Error-категории, а подробный exception/constraint остаётся в
логе. HTTP-контроллеры продолжают получать прежние Result/Envelope-типы.

## Idempotence and Recovery

Повторный build/test безопасен. Повторный вызов команды создаёт новую scoped
транзакцию и не использует состояние предыдущего запроса. При исключении
handler-а decorator выполняет rollback с `CancellationToken.None`, после чего
исключение либо возвращается через mapped Error для DB-ошибки, либо передаётся
существующему middleware для неожиданных ошибок. Миграции и данные не меняются.

## Artifacts and Notes

Добавлены `ITransactionManager`, `ITransactionalCommand`, три generic
декоратора и `TransactionManager`; удалены save-методы из `AppDbContext` и
репозиториев; validators и существующие handler-тесты синхронизированы с новой
границей commit.

## Interfaces and Dependencies

`ITransactionManager` находится в `Core.Abstractions`; реализация scoped
`TransactionManager` — в Infrastructure.Postgres и использует уже имеющиеся
EF Core/Npgsql зависимости. Scrutor регистрирует открытые generic-декораторы
для `ICommandHandler<,>`. Новые NuGet-пакеты и миграции не требуются.
