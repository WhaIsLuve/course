# DS-20: soft delete и фоновая очистка оргструктуры

Этот план описывает реализацию soft delete для оргструктуры и безопасной
фоновой очистки старых записей. План обновляется по мере выполнения.

## Цель

Удалённые локации, подразделения и должности остаются в PostgreSQL с отметкой
удаления, исчезают из всех текущих read-сценариев, а записи старше настроенного
срока удаляются фоновым процессом небольшими пакетами.

## Шаги

- [done] `done` — Создать ветку `feature/DS-20` и зафиксировать решение в плане.
- [done] `done` — Добавить состояние soft delete в домен, EF mapping, migration и global filters.
- [done] `done` — Перевести DELETE handlers/repositories и сохранить ограничения связей/иерархии.
- [done] `done` — Скрыть удалённые записи в EF и Dapper read-side, реализовать Position GET.
- [done] `done` — Добавить конфигурацию, batch purger, runner и BackgroundService.
- [done] `done` — Обновить unit/integration tests и выполнить полную проверку.

## Проверка

- [done] `done` — `dotnet build backend/DirectoryService/DirectoryService.slnx`.
- [done] `done` — Unit tests проекта `DirectoryService.UnitTests` (168/168).
- [done] `done` — Integration tests проекта `DirectoryService.IntegrationTests` (46/46) при доступном Docker.

## Решения

- DELETE связанной локации/должности сохраняет существующий `409`; физические
  строки связей на soft delete не меняются.
- DELETE подразделения с активными дочерними подразделениями возвращает
  `409 department.children.exist`.
- Новые активные Location/Position могут повторно использовать имя; индексы
  уникальности становятся частичными по `is_deleted = false`.
- Значения по умолчанию: запуск раз в сутки, retention 30 дней, batch 500.
- Восстановление и tree endpoint-ы не входят в DS-20; текущие EF/Dapper read
  сценарии обязаны скрывать удалённые строки.

## Журнал изменений

- 2026-09-08 — план создан, ветка `feature/DS-20` создана.
- 2026-09-09 — реализация завершена; из-за отсутствия Hosting API в Infrastructure
  hosted loop размещён в Web без добавления NuGet-пакета.
- 2026-09-09 — integration suite дважды выполнен с PostgreSQL Testcontainer;
  финальный прогон завершён 46/46.
- 2026-09-09 — глобальные EF-фильтры перенесены из `AppDbContext` в конфигурации
  сущностей; удалён неиспользуемый `using`, повторно пройдены build и тесты.
