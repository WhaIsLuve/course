# DS-17: Список подразделений с поиском, сортировкой и пагинацией

Этот ExecPlan фиксирует реализацию read-side списка подразделений. Он следует
`PLANS.MD`; автоматические тесты не добавляются по требованию задачи.

## Purpose / Big Picture

Администратор получает `GET /api/v1/departments` с плоскими строками таблицы,
поиском по имени, разрешённой сортировкой и offset-based пагинацией. `totalCount`
считается под тем же фильтром, что и текущая страница, поэтому UI может
корректно построить пагинатор.

## Progress

- [x] (2026-09-04) Добавлены `PagedResult<T>` и `DepartmentListItemDto`.
- [x] (2026-09-04) Реализованы query и handler через `IReadDbContext`.
- [x] (2026-09-04) Добавлены валидация, SQL-фильтрация, сортировка и проекция.
- [x] (2026-09-04) Endpoint подключён через `EndpointResult<T>`.
- [x] (2026-09-04) Сборка Web-проекта завершилась с 0 предупреждений и 0 ошибок.
- [ ] Ручной HTTP/SQL-прогон с PostgreSQL ещё не выполнен.

## Surprises & Discoveries

- В Core доступен стандартный EF Core, но не provider-specific extension Npgsql
  `ILike`. Для сохранения границы `IReadDbContext` фильтр реализован как
  `UPPER(name) LIKE UPPER(pattern)` с escape-символом; это остаётся SQL-запросом
  и даёт регистронезависимый поиск для русских и английских имён.
- Текущая модель проекта таргетирует `net10.0`; реализация не меняет target
  framework и не добавляет пакеты.
- Docker Engine в среде выполнения недоступен: `docker ps` завершился ошибкой
  доступа к `docker_engine`, поэтому HTTP/SQL-прогон отложен.

## Decision Log

- Решение: лимит `search` — 200 символов. Основание: согласовано перед
  реализацией как безопасный лимит поискового ввода.
- Решение: DTO содержит `Slug` и `Path` отдельными полями. Основание:
  сохраняется полная информация существующей карточки без раскрытия домена.
- Решение: одинаковые ключи сортируются по `Id ASC`. Основание: стабильный
  порядок необходим для offset-пагинации.
- Решение: миграции и индексы не добавляются. Основание: оптимизация индексов
  отложена следующей задачей.

## Outcomes & Retrospective

Контракт, read handler и endpoint реализованы; проект компилируется. Осталась
только проверка фактического HTTP-ответа и SQL-логов на доступном PostgreSQL.

## Context and Orientation

`DirectoryService.Contracts` содержит публичные DTO, `DirectoryService.Core`
содержит CQS query и `IReadDbContext`, а `DirectoryService.Web` — MVC-контроллер
и Envelope-результаты. `AppDbContext` уже предоставляет `Departments` через
read-only `IQueryable` с `AsNoTracking`; регистрация handler-ов выполняется
сканированием Core-сборки.

## Plan of Work

В `Contracts/Common/PagedResult.cs` добавлен обобщённый результат страницы, а в
`Contracts/Departments/DepartmentListItemDto.cs` — плоский DTO. В
`Core/Features/Departments/GetList` добавлены query и handler: handler проверяет
параметры, создаёт общий filtered query, отдельно вызывает `CountAsync`, затем
применяет whitelist сортировки, `Skip/Take` и `Select`. В контроллере GET-заглушка
заменена вызовом handler-а с дефолтами page 1 и pageSize 20. План и статус хранятся
в этом файле.

## Concrete Steps

Рабочая директория: `C:\Users\vlad\Projects\sachkovCource\course`.

    dotnet build backend/DirectoryService/src/DirectoryService.Web/DirectoryService.Web.csproj --no-restore

Ожидаемый результат: успешная сборка без предупреждений и ошибок. PostgreSQL
ручной прогон выполняется отдельно при доступном connection string.

## Validation and Acceptance

- Без параметров ответ содержит `page=1`, `pageSize=20`, `totalCount` всех строк
  и имя по возрастанию.
- При 25 строках запрос с `pageSize=20&page=2` возвращает 5 элементов и
  `totalCount=25`.
- `search` длиной до 200 символов ищет подстроку без учёта регистра; 201 символ
  возвращает Envelope с HTTP 400.
- `sortBy=name|createdAt`, `sortDir=asc|desc` работают; неизвестные значения,
  `page < 1` и `pageSize` вне диапазона 1..100 возвращают Envelope 400.
- Development SQL logging показывает отдельные `COUNT` и page SELECT с одинаковым
  фильтром; отсутствуют `Include`, N+1 и загрузка всей таблицы в память.

## Idempotence and Recovery

Изменения аддитивны и безопасны для повторной сборки. Для отката достаточно
удалить добавленные контракты, feature, план и вернуть GET-метод контроллера к
заглушке; миграции и существующие write/read-фичи не затрагиваются.

## Artifacts and Notes

Ключевые артефакты: `PagedResult<T>`, `DepartmentListItemDto`,
`GetDepartmentsQuery`, `GetDepartmentsHandler` и обновлённый
`DepartmentController`. Build evidence: `dotnet build ... --no-restore` — 0
warnings, 0 errors.

## Interfaces and Dependencies

- `GetDepartmentsQuery` — `Search`, `SortBy`, `SortDir`, `Page`, `PageSize`;
  значения по умолчанию: `name`, `asc`, `1`, `20`.
- Handler — `IQueryHandler<GetDepartmentsQuery,
  Result<PagedResult<DepartmentListItemDto>, Error>>`.
- Endpoint — `EndpointResult<PagedResult<DepartmentListItemDto>>`.
- Используются существующие `IReadDbContext` и `Microsoft.EntityFrameworkCore`;
  новые NuGet-пакеты, миграции и индексы не нужны.
