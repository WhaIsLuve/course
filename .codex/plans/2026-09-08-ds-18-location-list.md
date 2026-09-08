# DS-18: Список локаций через Dapper с агрегатом подразделений

Этот план фиксирует реализацию read-side списка локаций и следует `PLANS.MD`.

## Purpose / Big Picture

Администратор получает `GET /api/v1/locations` с названием, плоским адресом,
датой создания и количеством связанных подразделений. Результат поддерживает
поиск, фильтр по агрегату, whitelist-сортировку и offset-пагинацию с корректным
`totalCount`.

## Progress

- [x] (2026-09-08) Добавлены query, плоский DTO и Dapper handler.
- [x] (2026-09-08) Подключены validation, CTE, count/page и whitelist ORDER BY.
- [x] (2026-09-08) GET endpoint подключён через `EndpointResult<T>`.
- [x] (2026-09-08) Web-проект собран: 0 предупреждений, 0 ошибок.
- [x] (2026-09-08) Выполнены ручные HTTP-проверки дефолтов, фильтров,
  сортировок, пагинации и валидационных ошибок.
- [x] (2026-09-08) Выполнен `EXPLAIN (ANALYZE, BUFFERS)` на тестовой БД;
  вывод и интерпретация записаны ниже.

## Surprises & Discoveries

- Existing `LocationController` already owns `/api/v1/locations`; its GET action
  was a stub, so no route change is needed.
- Dapper 2.1.66 and `IDbConnectionFactory` are already available; no dependency
  or schema change is required.

## Decision Log

- Решение: адрес остаётся плоским (`Country`, `City`, `Street`, `Building`),
  как в существующих location read-контрактах. Дата/автор: 2026-09-08 / Codex.
- Решение: `COUNT(DISTINCT dl.department_id)` защищает агрегат от возможных
  повторных связей. Дата/автор: 2026-09-08 / Codex.
- Решение: для `ILIKE` используется `~` как SQL escape-символ; сам `~`, `%` и
  `_` экранируются перед передачей параметра. Это не смешивает пользовательский
  обратный слэш с синтаксисом PostgreSQL. Дата/автор: 2026-09-08 / Codex.
- Решение: count и страница возвращаются одним SQL через общую CTE-основу и
  `LEFT JOIN`, чтобы count сохранялся для пустых страниц. Дата/автор:
  2026-09-08 / Codex.

## Outcomes & Retrospective

GET `/api/v1/locations` возвращает требуемый Envelope с плоскими строками,
агрегатом и `totalCount`; Dapper SQL использует общую CTE-основу для count и
страницы. Сборка и ручные проверки успешны. На небольшой тестовой БД план
выполнился за 0.947 ms; индексы и дальнейшая оптимизация намеренно не менялись.

## Context and Orientation

Контракты находятся в `DirectoryService.Contracts`, CQS-запросы и handler-ы — в
`DirectoryService.Core`, а MVC endpoint — в `DirectoryService.Web`. Таблицы
Postgres называются `locations` и `department_locations`; адрес хранится
колонками `country`, `city`, `street`, `building`.

## Plan of Work

`LocationListItemDto` описывает строку UI. `GetLocationsQuery` принимает
`search`, `minDepartmentCount`, `sortBy`, `sortDir`, `page`, `pageSize`.
`GetLocationsHandler` валидирует вход, экранирует LIKE-шаблон, подставляет в
SQL только заранее разрешённые выражение колонки и направление, а остальные
значения передаёт через Dapper parameters. CTE считает distinct department ids,
применяет поиск и HAVING, затем строит total и страницу из одной основы.

## Concrete Steps

Рабочая директория: `C:\Users\vlad\Projects\sachkovCource\course`.

    dotnet build backend/DirectoryService/src/DirectoryService.Web/DirectoryService.Web.csproj --no-restore
    docker compose -f backend/DirectoryService/docker/docker-compose.yml up -d postgres_course
    dotnet run --project backend/DirectoryService/src/DirectoryService.Web/DirectoryService.Web.csproj

Ожидаемый HTTP URL: `http://localhost:5002/api/v1/locations`.

## Validation and Acceptance

Проверить запрос без параметров, `search`, `minDepartmentCount`, все whitelist
сочетания сортировки, вторую страницу, пустой результат и страницу за пределами
данных. `page < 1`, `pageSize` вне 1..100, отрицательный minimum, неизвестные
sort values и search длиной 201 символ должны дать Envelope с HTTP 400.

Для финального SQL выполнить `EXPLAIN (ANALYZE, BUFFERS)` на тестовых данных и
сохранить полный вывод в PR или commit message; отдельно указать операторы
фильтра, агрегации/HAVING, сортировки и LIMIT/OFFSET.

## Idempotence and Recovery

Сборку и read-only SQL/HTTP проверки можно повторять. Изменения не добавляют
миграций и индексов; безопасный откат — удалить добавленный feature/DTO и
вернуть GET action к прежней заглушке.

## Artifacts and Notes

Ключевые артефакты: `LocationListItemDto`, `GetLocationsQuery`,
`GetLocationsHandler`, обновлённый `LocationController` и этот план.

Ручная проверка: дефолтный запрос вернул HTTP 200, 10 элементов и
`totalCount=10`; поиск `loc_1` с `minDepartmentCount=2` вернул одну строку;
`page=2&pageSize=4` вернул четыре строки при `totalCount=10`; пустой фильтр
вернул HTTP 200 с пустым `items` и `totalCount=0`. `sortBy=name|createdAt|
departmentCount` в обоих направлениях дал монотонный порядок. `sortBy`,
`sortDir`, `page=0`, `pageSize=101`, `minDepartmentCount=-1` и search длиной
201 символ дали HTTP 400 с общим Envelope. Поиск с `%` и `\\%` вернул пустую
выборку без ошибки, подтвердив экранирование wildcard-символов.

Фактический `EXPLAIN (ANALYZE, BUFFERS)` точного параметризованного SQL handler-а
для поиска `%LOC%`, minimum `1`, сортировки `DepartmentCount DESC`,
`LIMIT 20 OFFSET 0`:

    Nested Loop Left Join  (cost=29.70..29.73 rows=1 width=1822) (actual time=1.239..1.245 rows=5 loops=1)
      Buffers: shared hit=21 read=1
      CTE filtered_locations
        ->  GroupAggregate  (cost=27.21..29.65 rows=1 width=1818) (actual time=1.071..1.094 rows=5 loops=1)
              Group Key: l.id
              Filter: (count(DISTINCT dl.department_id) >= 1)
              Rows Removed by Filter: 5
              Buffers: shared hit=18 read=1
            ->  Sort  (cost=27.21..28.02 rows=323 width=1830) (actual time=1.060..1.063 rows=18 loops=1)
                    Sort Key: l.id, dl.department_id
                    Sort Method: quicksort  Memory: 26kB
                    Buffers: shared hit=18 read=1
                ->  Nested Loop Left Join  (cost=4.19..13.75 rows=323 width=1830) (actual time=0.852..0.914 rows=18 loops=1)
                          Buffers: shared hit=15 read=1
                    ->  Seq Scan on locations l  (cost=0.00..1.04 rows=1 width=1814) (actual time=0.019..0.036 rows=10 loops=1)
                                Filter: ((name)::text ~~* '%LOC%'::text)
                                Buffers: shared hit=1
                          ->  Bitmap Heap Scan on department_locations dl  (cost=4.19..12.66 rows=5 width=32) (actual time=0.085..0.085 rows=1 loops=10)
                                Recheck Cond: (location_id = l.id)
                                Heap Blocks: exact=5
                                Buffers: shared hit=14 read=1
                                ->  Bitmap Index Scan on "IX_department_locations_location_id"  (cost=0.00..4.19 rows=5 width=0) (actual time=0.083..0.083 rows=1 loops=10)
                                      Index Cond: (location_id = l.id)
                                      Buffers: shared hit=9 read=1
    ->  Aggregate  (cost=0.02..0.04 rows=1 width=4) (actual time=1.101..1.101 rows=1 loops=1)
            Buffers: shared hit=18 read=1
        ->  CTE Scan on filtered_locations  (cost=0.00..0.02 rows=1 width=0) (actual time=1.074..1.098 rows=5 loops=1)
                  Buffers: shared hit=18 read=1
    ->  Limit  (cost=0.03..0.04 rows=1 width=1818) (actual time=0.135..0.137 rows=5 loops=1)
            Buffers: shared hit=3
        ->  Sort  (cost=0.03..0.04 rows=1 width=1818) (actual time=0.135..0.136 rows=5 loops=1)
              Sort Key: filtered_locations_1."DepartmentCount" DESC, filtered_locations_1."Id"
                  Sort Method: quicksort  Memory: 25kB
                  Buffers: shared hit=3
              ->  CTE Scan on filtered_locations filtered_locations_1  (cost=0.00..0.02 rows=1 width=1818) (actual time=0.001..0.002 rows=5 loops=1)
        Planning:
          Buffers: shared hit=197
        Planning Time: 12.229 ms
        Execution Time: 1.546 ms

Интерпретация: `Seq Scan` фильтрует имя через `ILIKE`; `Bitmap Index Scan`
читает связанные строки по `location_id`; `GroupAggregate` считает distinct
подразделения и применяет `HAVING`; `CTE Scan` повторно используется для count
и страницы; финальные `Sort` и `Limit` применяют whitelist-порядок и offset-
пагинацию.

## Interfaces and Dependencies

- `GetLocationsQuery` реализует `IQuery<Result<PagedResult<LocationListItemDto>, Error>>`.
- Handler реализует соответствующий `IQueryHandler` и получает
  `IDbConnectionFactory` через DI.
- Используются существующие Dapper, `PagedResult<T>`, `Envelope<T>` и
  `EndpointResult<T>`; новые NuGet-пакеты не нужны.
