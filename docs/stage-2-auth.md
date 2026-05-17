# Этап 2 — Аутентификация: seed Head + register/login/logout/me + Bearer middleware

**Дата:** 2026-05-17
**Цель:** работающая авторизация по Bearer-токену из БД, сидование Head из конфига, 5 эндпоинтов аутентификации.

## Что создано

### Сервисы
- [Services/PasswordHasher.cs](../ZAMETKI.Api/Services/PasswordHasher.cs) — обёртка над BCrypt: `Hash(password)`, `Verify(password, hash)`.
- [Services/TokenService.cs](../ZAMETKI.Api/Services/TokenService.cs) — `CreateSessionAsync(userId)`, `GetUserBySessionAsync(token)`, `DeleteSessionAsync(token)`. TTL **30 суток**.
- [Services/SeedService.cs](../ZAMETKI.Api/Services/SeedService.cs) — при старте API создаёт Head из `appsettings:HeadAdmin`, если в `Users` нет ни одного Head. Идемпотентно.

### Middleware и DTO
- [Middleware/BearerAuthMiddleware.cs](../ZAMETKI.Api/Middleware/BearerAuthMiddleware.cs) — читает заголовок `Authorization: Bearer <guid>`, лукапит сессию в БД, кладёт `User` и `Token` в `HttpContext.Items`. Без токена — просто пропускает дальше; решение о доступе принимает сам контроллер. Рядом extension'ы `HttpContext.GetUser()` / `GetToken()`.
- [Contracts/AuthDtos.cs](../ZAMETKI.Api/Contracts/AuthDtos.cs) — `RegisterStudentRequest`, `RegisterTeacherRequest`, `LoginRequest`, `UserDto`, `AuthResponse`.

### Контроллер
- [Controllers/AuthController.cs](../ZAMETKI.Api/Controllers/AuthController.cs) — 5 эндпоинтов:

| Метод | Путь | Тело | Доступ | Ответ |
|-------|------|------|--------|-------|
| POST | `/api/auth/register/student` | `{groupName, fullName, password}` | anon | 200 `{token, user}` / 403 «не приглашён» |
| POST | `/api/auth/register/teacher` | `{fullName, password}` | anon | 200 / 409 |
| POST | `/api/auth/login` | `{fullName, password}` | anon | 200 / 401 |
| POST | `/api/auth/logout` | — | auth | 204 |
| GET | `/api/auth/me` | — | auth | 200 `UserDto` / 401 |

### Program.cs (обновлён)
- DI: `PasswordHasher` (Singleton), `TokenService` и `SeedService` (Scoped).
- `app.UseMiddleware<BearerAuthMiddleware>()`.
- На старте после миграций — `SeedService.SeedAsync()`.

## Логика регистрации студента

1. Ищем `Group` по введённому `GroupName`.
2. Если группы нет → **403** `«Вас не пригласили в эту группу.»`.
3. Если есть — ищем `GroupMember` с `StudentFullName==fullName` и `Status==Invited` в этой группе.
4. Если нет — **403** (то же сообщение).
5. Если есть — создаём `User(Role=Student)`, проставляем `invite.StudentId` и `invite.Status=Active`, создаём `Session`.
6. Возвращаем `{token, user}`.

## Архитектурные решения

- **Bearer-токен в БД, а не JWT.** Совпадает с формулировкой ТЗ «токен сессии», проще для учебного MVP, не требует секрета/подписи. Логаут моментальный (удаление строки).
- **Middleware кладёт `User` в `Items`, но не выкидывает 401.** Контроллеры явно решают, нужна ли авторизация — это упростит работу без auth-схемы ASP.NET (которая иначе пыталась бы редиректить).
- **Head создаётся seed-ом из appsettings.** По ТЗ — не саморегистрируется; для 1 университета достаточно сидования из конфига. Пароль и ФИО легко переопределить через `appsettings.Local.json`.
- **Один сообщение для двух разных ошибок («группа не существует» и «не приглашён»)** — чтобы не давать злоумышленнику инфу о существовании групп.

## Проверка (9/9 ✓)

```bash
# 1. Head login (seeded)
POST /api/auth/login {"fullName":"Заведующий","password":"head-admin"}        → 200 + token

# 2. /me с токеном
GET /api/auth/me  Authorization: Bearer <token>                                → 200 UserDto

# 3. Регистрация препода
POST /api/auth/register/teacher {"fullName":"Иванов И.И.","password":"pw1"}    → 200 + token

# 4. Повторная регистрация
POST /api/auth/register/teacher (same body)                                    → 409

# 5. Регистрация студента без приглашения
POST /api/auth/register/student {"groupName":"ИВТ-21","fullName":"Петров П.П.","password":"pw"}
                                                                                → 403 «Вас не пригласили в эту группу.»

# 6. Логин с неверным паролем
POST /api/auth/login {"fullName":"Заведующий","password":"wrong"}              → 401

# 7. Logout
POST /api/auth/logout Authorization: Bearer <token>                            → 204

# 8. /me после logout (токен удалён)
GET /api/auth/me Authorization: Bearer <token>                                 → 401

# 9. /me без токена
GET /api/auth/me                                                                → 401
```

Сценарий «студент **с** приглашением» проверится на этапе 3, когда появятся эндпоинты создания группы и приглашений.

## Результат

Сервер умеет принимать пользователей и выдавать токены. Сценарии регистрации/входа/выхода работают; роли разделены; Head существует с момента первого старта. Готова почва для бизнес-эндпоинтов (заметки, группы).
