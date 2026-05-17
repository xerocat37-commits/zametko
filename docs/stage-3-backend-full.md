# Этап 3 — Бэкенд: Notes (Personal + Feed), Groups, GroupMembers, Group/Global notes

**Дата:** 2026-05-17
**Цель:** закрыть весь бэкенд функционала ТЗ за один этап, чтобы клиент дальше делался против стабильного API.

## Что создано

### DTO
- [Contracts/NoteDtos.cs](../ZAMETKI.Api/Contracts/NoteDtos.cs) — `NoteDto`, `CreateNoteRequest`, `UpdateNoteRequest`.
- [Contracts/GroupDtos.cs](../ZAMETKI.Api/Contracts/GroupDtos.cs) — `GroupDto`, `CreateGroupRequest`.
- [Contracts/GroupMemberDtos.cs](../ZAMETKI.Api/Contracts/GroupMemberDtos.cs) — `GroupMemberDto`, `InviteStudentRequest`.

### Авторизация
- [Authorization/RoleGuard.cs](../ZAMETKI.Api/Authorization/RoleGuard.cs) — `RequireAuth(ctx)`, `RequireRole(ctx, params roles)`, `Forbidden()`.

### Контроллеры
- [Controllers/NotesController.cs](../ZAMETKI.Api/Controllers/NotesController.cs) — Personal CRUD, Feed, Global create.
- [Controllers/GroupsController.cs](../ZAMETKI.Api/Controllers/GroupsController.cs) — управление группами + групповые заметки.
- [Controllers/GroupMembersController.cs](../ZAMETKI.Api/Controllers/GroupMembersController.cs) — приглашения.

## API (полный список этапа)

| Метод | Путь | Доступ | Назначение |
|-------|------|--------|-----------|
| GET | `/api/notes` | any | мои Personal |
| POST | `/api/notes` | any | создать Personal |
| PUT | `/api/notes/{id}` | владелец | редактировать Personal |
| DELETE | `/api/notes/{id}` | владелец | удалить Personal |
| GET | `/api/notes/feed` | any | Global + (Student) Group моих групп / (Teacher) Group мои |
| POST | `/api/notes/global` | Head | создать рассылку |
| GET | `/api/groups/my` | Teacher | мои группы |
| POST | `/api/groups` | Teacher | создать группу |
| GET | `/api/groups/{id}` | владелец / член | детали |
| GET | `/api/groups/{id}/members` | владелец | список приглашённых |
| POST | `/api/groups/{id}/members` | владелец | пригласить |
| GET | `/api/groups/{id}/notes` | владелец / active-член | групповые заметки |
| POST | `/api/groups/{id}/notes` | владелец | создать групповую |

## Логика `/api/notes/feed`

Лента собирается из двух источников:
- Все `NoteType=Global` — видят все.
- Дополнительно по роли:
  - **Student** — `NoteType=Group` где `TargetGroupId` входит в группы пользователя (по `GroupMembers.StudentId=я AND Status=Active`).
  - **Teacher** — `NoteType=Group` где `OwnerId=я` (свои групповые заметки).
  - **Head** — только Global (групповых нет в логике Head).

Сортировка — по `CreatedDate DESC`.

## Архитектурные решения

- **`RoleGuard` вместо `[Authorize]`.** В этапе 2 решено использовать собственный middleware без auth-схемы; стандартный `[Authorize]` без схемы не работает. Контроллеры явно вызывают `RequireAuth/RequireRole` в начале метода.
- **Замена `Forbid()` → `RoleGuard.Forbidden()`.** Встроенный `Forbid()` без auth-схемы кидает 500. `Forbidden()` возвращает `ObjectResult` с кодом 403 и телом `{ error }`. Заменено везде в трёх контроллерах.
- **Группа-заметки — в `GroupsController`, а не отдельный `GroupNotesController`.** REST-путь `/api/groups/{id}/notes` логически принадлежит ресурсу `Group`; отдельный контроллер был бы микро-классом без выгоды.
- **`GET /api/notes` отдаёт только Personal текущего юзера.** Если бы возвращало все типы — клиенту пришлось бы фильтровать локально; явное разделение Personal/Feed на сервере проще и безопаснее.
- **Конфликт уникальности имени группы.** При попытке создать группу с уже существующим именем — 409. На случай если параллельно работают несколько преподов в учебной группе.

## Проверка

### Сквозной E2E (curl)
```
Head logs in → POST /api/notes/global "Рассылка №1" → 200
Teacher registers → POST /api/groups "ИВТ-21" → видит в /api/groups/my → 200
Teacher invites "Петров П.П." → POST /api/groups/{id}/members → 200 (Status=Invited)
Teacher creates POST /api/groups/{id}/notes "Лекция 5" → 200
Student "Петров П.П." registers with groupName=ИВТ-21 → 200
GET /api/groups/{id}/members → Петров теперь Active, StudentId проставлен
Student GET /api/notes/feed → видит и "Рассылка №1", и "Лекция 5"
Student POST /api/notes → создаёт Personal
Teacher GET /api/notes → []  (чужие Personal не видит)
Student PUT /api/notes/{id} → 200
Student DELETE /api/notes/{id} → 204

Другой студент без приглашения → POST /api/auth/register/student → 403
```

### Проверки авторизации
- Препод → `POST /api/notes/global` → 403.
- Head → `POST /api/groups` → 403.
- Студент → `POST /api/groups` → 403.
- Препод → `PUT/DELETE` чужой Personal → 403.
- Студент → `GET /api/groups/my` → 403.
- `/me` без токена → 401.

## Что было найдено и исправлено

**Баг с `Forbid()` → 500.** Первый прогон вернул `HTTP 500` на всех проверках авторизации. Причина: в ASP.NET Core `Forbid()` пытается вызвать `IAuthenticationService.ForbidAsync()`, которая без зарегистрированной auth-схемы кидает исключение. Решение: ввёл `RoleGuard.Forbidden()` → возвращает явный `ObjectResult` с `StatusCode=403`. Заменил `return Forbid();` во всех контроллерах через `Edit` с `replace_all`.

## Результат

Бэкенд закрыт полностью. Все 13 эндпоинтов отвечают корректно. ТЗ-сценарий «Head рассылает → препод создаёт группу → приглашает → студент регистрируется и видит обе записи в ленте» — работает end-to-end.
