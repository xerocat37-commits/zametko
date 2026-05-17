# Этап 4 — Клиент: HttpClient, ApiClient, AuthService, DI, реструктуризация

**Дата:** 2026-05-17
**Цель:** инфраструктура клиента готова, UI ещё старый (локальный SQLite-CRUD), сборка зелёная.

## Что создано

### Пакеты
- В [ZAMETKI/ZAMETKI.csproj](../ZAMETKI/ZAMETKI.csproj) добавлен `CommunityToolkit.Mvvm` 8.4.0 (для VM на следующих этапах).
- Старые SQLite-пакеты пока **оставлены** — `MainPage` со старым `NoteService` ещё должен компилироваться.

### DTO (зеркало серверных)
- [ZAMETKI/Api/Contracts/AuthDtos.cs](../ZAMETKI/Api/Contracts/AuthDtos.cs) — `RegisterStudentRequest`, `RegisterTeacherRequest`, `LoginRequest`, `UserDto`, `AuthResponse`.
- [ZAMETKI/Api/Contracts/NoteDtos.cs](../ZAMETKI/Api/Contracts/NoteDtos.cs) — `NoteDto`, `CreateNoteRequest`, `UpdateNoteRequest` + константы `NoteTypes`.
- [ZAMETKI/Api/Contracts/GroupDtos.cs](../ZAMETKI/Api/Contracts/GroupDtos.cs) — `GroupDto`, `GroupMemberDto`, `CreateGroupRequest`, `InviteStudentRequest` + константы `UserRoles`, `GroupMemberStatus`.

### HTTP-инфраструктура
- [ZAMETKI/Api/ApiClient.cs](../ZAMETKI/Api/ApiClient.cs) — обёртка над `HttpClient`:
  - `GetAsync<T>(path)`, `PostAsync<T>(path, body)`, `PostAsync(path, body)` (без тела ответа), `PutAsync<T>`, `DeleteAsync`.
  - Автоматически подставляет `Authorization: Bearer <Token>` если токен задан.
  - System.Text.Json с `PropertyNameCaseInsensitive=true`.
  - При не-успешном HTTP-коде извлекает поле `error` или `title` из тела и кидает `ApiException(statusCode, message)`.
- [ZAMETKI/Api/ApiException.cs](../ZAMETKI/Api/ApiException.cs) — `StatusCode` + `Message`.

### Сервисы
- [ZAMETKI/Services/AppConfig.cs](../ZAMETKI/Services/AppConfig.cs) — `ApiBaseUrl = "http://localhost:5000"`.
- [ZAMETKI/Services/AuthService.cs](../ZAMETKI/Services/AuthService.cs):
  - Свойства `Token`, `CurrentUser`, `IsAuthenticated`.
  - `LoginAsync`, `RegisterStudentAsync`, `RegisterTeacherAsync`, `LogoutAsync`.
  - `RestoreAsync()` — читает токен из `SecureStorage`, дёргает `/api/auth/me`; при 401 чистит SecureStorage.
  - Событие `AuthStateChanged` (на него подпишется AppShell на этапе 6).

### DI
- В [ZAMETKI/MauiProgram.cs](../ZAMETKI/MauiProgram.cs):
  - `HttpClient` с `BaseAddress = ApiBaseUrl` (Singleton).
  - `ApiClient` (Singleton).
  - `AuthService` (Singleton).

## Архитектурные решения

- **`ApiClient` как Singleton, без `IHttpClientFactory`.**
  Изначально хотел использовать `services.AddHttpClient<ApiClient>(...)`, но это регистрирует `ApiClient` как **transient** — токен на нём терялся бы между разрешениями. В Singleton токен живёт всю сессию, что и нужно. `HttpClient` тоже Singleton — для одного `BaseAddress` пул соединений общий.
- **DTO дублируются.** Не создаём общий `ZAMETKI.Shared` проект — для учебного MVP это лишний csproj и TFM-конфликты (MAUI vs WebAPI). 5–7 record-ов синхронизируются вручную.
- **`AuthService` владеет токеном.** Он же кладёт его в `ApiClient.Token` и в `SecureStorage`. Никто другой не пишет в `SecureStorage`. Это даёт одну точку входа для логина/логаута/восстановления.
- **`RestoreAsync` обрабатывает 401 тихо.** Если токен из SecureStorage уже невалиден на сервере (был удалён, истёк) — просто чистим локально и возвращаем `false`. Пользователь увидит экран Login без алёрта.

## Проверка

```bash
dotnet build ZAMETKI/ZAMETKI.csproj -f net9.0-windows10.0.19041.0
```

Результат: 0 ошибок, 3 предупреждения XamlC XC0022 на существующем `MainPage.xaml` (про compiled bindings — не от моих изменений, будет переписан на этапе 6).

Старый `MainPage` + `NoteService` + `Note(int Id)` продолжают работать без изменений — локальный SQLite-CRUD не сломан.

## Результат

Инфраструктура для UI готова. На следующем этапе можно добавлять страницы Login/Register, не отвлекаясь на HTTP-механику.
