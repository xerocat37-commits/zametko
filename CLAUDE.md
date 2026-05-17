# ZAMETKI — заметки о проекте

Учебное MVP-приложение для одного университета: личные / групповые / общие заметки, роли Студент / Преподаватель / Заведующий. Клиент-серверная архитектура: .NET MAUI клиент + ASP.NET Core Web API + SQLite (WAL) на сервере.

## Стек

- **Клиент**: .NET MAUI, .NET 9. TargetFrameworks: `net9.0-windows10.0.19041.0`, `-android`, `-ios`, `-maccatalyst`. Активный отладочный TF — Windows (`ZAMETKI/Properties/launchSettings.json`). MVVM на `CommunityToolkit.Mvvm` 8.4.0. HTTP через собственную обёртку `ApiClient` (Singleton). Токен сессии хранится в `SecureStorage`.
- **API**: ASP.NET Core Web API в `ZAMETKI.Api/`, .NET 9. Слушает `http://localhost:5000` (без HTTPS для учебного MVP).
- **БД**: SQLite в режиме WAL, файл `ZAMETKI.Api/zametki.db` (в `.gitignore`). ORM — **EF Core** (`Microsoft.EntityFrameworkCore.Sqlite` 9.0.0 + `.Design` для миграций). Миграции в `ZAMETKI.Api/Data/Migrations/`, применяются автоматически на старте через `db.Database.Migrate()`.
- **Аутентификация**: BCrypt-хеш пароля (`BCrypt.Net-Next` 4.0.3) + простой Bearer-токен в таблице `Sessions` (TTL 30 суток). На клиенте — `Authorization: Bearer <guid>`.
- **SDK у разработчика**: .NET 9.0.314, workload `maui` 9.0.120/9.0.100, `dotnet-ef` 10+.

## Карта репозитория

- `ZAMETKI.sln` — два проекта: `ZAMETKI/` и `ZAMETKI.Api/`.
- `ZAMETKI/` — клиент .NET MAUI (Single-Project, `<UseMaui>true</UseMaui>`).
  - `Api/` — `ApiClient.cs` (HttpClient-обёртка), `ApiException.cs`, `Contracts/` (зеркало серверных DTO + константы `UserRoles`, `NoteTypes`, `GroupMemberStatus`).
  - `Services/` — `AppConfig` (URL API), `AuthService` (login/register/logout/restore, токен в SecureStorage), `NotesApi`, `GroupsApi`, `GroupMembersApi`, `GroupNotesApi`, `GlobalNotesApi`.
  - `ViewModels/` — `LoginViewModel`, `RegisterStudentViewModel`, `RegisterTeacherViewModel`, `PersonalNotesViewModel`, `FeedViewModel`, `MyGroupsViewModel`, `GroupDetailsViewModel`, `GroupNoteEditorViewModel`, `HeadBroadcastViewModel`. Все — на `ObservableObject` + `[ObservableProperty]` + `[RelayCommand]`.
  - `Pages/` — `LoginPage`, `RoleSelectionPage`, `RegisterStudentPage`, `RegisterTeacherPage`, `PersonalNotesPage`, `FeedPage`, `MyGroupsPage`, `GroupDetailsPage`, `GroupNoteEditorPage`, `HeadBroadcastPage`.
  - `Resources/` — **только** ассеты MAUI: `AppIcon`, `Fonts`, `Images`, `Raw`, `Splash`, `Styles`.
  - `Platforms/<Android|iOS|MacCatalyst|Tizen|Windows>/` — платформо-зависимый код.
  - `Properties/launchSettings.json` — профиль запуска «Windows Machine».
  - `App.xaml`, `AppShell.xaml` — Shell с двумя root: `login` и `main` (FlyoutItem с вкладками Личные / Лента / Мои группы / Рассылка). Видимость «Мои группы» (Teacher) и «Рассылка» (Head) переключает `AppShell.xaml.cs` по `AuthService.CurrentUser.Role`.
  - `GlobalXmlns.cs` — мапит `ZAMETKI` и `ZAMETKI.Pages` на дефолтный XAML-неймспейс MAUI.
- `ZAMETKI.Api/` — ASP.NET Core Web API.
  - `Domain/` — POCO: `User`, `Group`, `GroupMember`, `Note`, `Session` + константы `UserRoles`, `NoteTypes`, `GroupMemberStatus`.
  - `Data/AppDbContext.cs` — DbSet'ы и индексы.
  - `Data/Migrations/` — миграция `Init`.
  - `Contracts/` — DTO (Auth, Note, Group, GroupMember).
  - `Controllers/` — `AuthController`, `NotesController`, `GroupsController`, `GroupMembersController`.
  - `Services/` — `PasswordHasher`, `TokenService`, `SeedService` (создаёт Head из `appsettings:HeadAdmin`).
  - `Middleware/BearerAuthMiddleware.cs` — читает `Authorization: Bearer` и кладёт `User` в `HttpContext.Items`.
  - `Authorization/RoleGuard.cs` — `RequireAuth`, `RequireRole`, `Forbidden()`.
  - `appsettings.json` — ConnectionString + `HeadAdmin: { FullName, Password }`.
- `ТЗ_Онлайн-заметки_MVP.md` — техническое задание.

## Запуск

Нужно запустить **два процесса**: API и клиент.

| Действие | Команда |
|----------|---------|
| Сборка solution | `dotnet build` |
| Запуск сервера API | `dotnet run --project ZAMETKI.Api/ZAMETKI.Api.csproj` (слушает `http://localhost:5000`) |
| Сборка только под Windows | `dotnet build ZAMETKI/ZAMETKI.csproj -f net9.0-windows10.0.19041.0` |
| Запуск Windows-клиента | `dotnet build ZAMETKI/ZAMETKI.csproj -t:Run -f net9.0-windows10.0.19041.0` (или F5 в VS, профиль «Windows Machine») |
| Новая EF-миграция | `cd ZAMETKI.Api && dotnet ef migrations add <Name> --output-dir Data/Migrations` |
| Сброс БД (для тестов) | `rm ZAMETKI.Api/zametki.db*` (миграции применятся при следующем старте) |
| Проверка workload'ов | `dotnet workload list` (нужен `maui` ≥ 9.0.120) |
| Установка workload'а | `dotnet workload install maui` |

Тестового проекта в репо нет.

### Учётные данные Head по умолчанию

Из `ZAMETKI.Api/appsettings.json`:
- ФИО: `Заведующий`
- Пароль: `head-admin`

Меняй в `appsettings.json` или переопределяй через `appsettings.Local.json` (в .gitignore).

## Архитектура

`[.NET MAUI клиент] ──REST/HTTP (Bearer)──> [ASP.NET Core Web API] ──EF Core──> [SQLite WAL на сервере]`

- **SQLite живёт на сервере**, не на клиенте. Клиент хранит только токен сессии в `SecureStorage` и не имеет локальной БД.
- **3 роли**: Student (саморегистрация: группа + ФИО + пароль), Teacher (саморегистрация: ФИО + пароль), **Head создаётся только seed-ом** при первом старте API из `appsettings:HeadAdmin` (не саморегистрируется).
- **Источник истины членства в группе** — таблица `GROUP_MEMBER`, а не `USER.GroupName`. `GroupName` в `USER` — лишь то, что ввёл студент при регистрации.
- **Приглашение до регистрации**: `GROUP_MEMBER` хранит `StudentFullName` + nullable `StudentId` + `Status` (Invited/Active). При регистрации студента сервер ищет матч по ФИО+группе и переводит `Invited` → `Active`, проставляя `StudentId`.
- **Авторизация**: на каждый запрос middleware читает `Authorization: Bearer`, лукапит `Session` в БД, кладёт `User` в `HttpContext.Items`. Контроллеры явно вызывают `RoleGuard.RequireAuth` / `RoleGuard.RequireRole`.

Полная модель данных и сценарии — в [ТЗ_Онлайн-заметки_MVP.md](ТЗ_Онлайн-заметки_MVP.md).

## Соглашения

- MAUI Single-Project: один csproj, мульти-таргет. Платформо-зависимый код — только в `Platforms/<Platform>/`.
- Все строки UI и сообщений — на русском.
- Неймспейсы плоские: `ZAMETKI.Services`, `ZAMETKI.ViewModels`, `ZAMETKI.Pages`, `ZAMETKI.Api`, `ZAMETKI.Api.Contracts` — независимо от подпапки. `GlobalXmlns.cs` маппит `ZAMETKI` и `ZAMETKI.Pages` на дефолтный XAML-неймспейс — добавляя страницу в `ZAMETKI.Pages`, импорт в XAML не нужен.
- Имя файла = имя основного типа в файле.
- DTO на клиенте и сервере дублируются (записи в `ZAMETKI/Api/Contracts/` и `ZAMETKI.Api/Contracts/`). Это осознанный выбор для учебного MVP вместо общего проекта `ZAMETKI.Shared`. При изменении DTO обновлять обе стороны.
- VM на `CommunityToolkit.Mvvm`: поля с `[ObservableProperty]`, команды через `[RelayCommand]`. Для пары IsBusy/IsNotBusy — `[NotifyPropertyChangedFor(nameof(IsNotBusy))]` + ручное `public bool IsNotBusy => !IsBusy;`.
- Страницы и VM регистрируются в DI (`MauiProgram.cs`) как Transient, сервисы (`ApiClient`, `AuthService`, `*Api`, `AppShell`) — как Singleton.

## Правила

- **Не возвращать `Forbid()` в контроллерах** — без настроенной auth-схемы он бросает 500. Использовать `RoleGuard.Forbidden()` (ручной 403 с телом `{ error }`).
- **Не плодить локальный-БД-код на клиенте**. Источник истины — сервер. Любая новая сущность сначала: серверная модель + endpoint, затем клиентский DTO + HTTP-вызов.
- **Не коммитить `bin/`, `obj/`, `.vs/`, `*.user`, `ZAMETKI.Api/zametki.db*`** — они в `.gitignore`. Если появились в `git status`, проверь `.gitignore` и `git rm --cached <путь>` перед коммитом.
- **Не хранить секреты в репо** (пароли, токены, ключи API). Для локальной разработки клиента — `dotnet user-secrets`; для сервера — `appsettings.Local.json` (в gitignore) или переменные окружения. `appsettings.Development.json` тоже в gitignore.
- **Не менять схему БД молча**. Любое изменение модели — через `dotnet ef migrations add <Name>`.
- **Не предлагать саморегистрацию для роли Head** на экране регистрации; на `RoleSelectionPage` только Student / Teacher. Head создаётся seed-ом.
- **Не класть C#-код в `Resources/`** — это папка для ассетов MAUI. Новые классы — в `ZAMETKI/Services/`, `ZAMETKI/Pages/`, `ZAMETKI/ViewModels/`, `ZAMETKI/Api/`.
- **Не возвращать `null` из `ApiClient`-методов** — он бросает `ApiException(statusCode, message)` при не-успешном HTTP-коде; вызывающий код ловит и показывает через `Shell.Current.DisplayAlert`.
- **Не запускать только клиент без API** — клиент сразу попытается дёрнуть `/api/auth/me` через `RestoreAsync` и упадёт в `ApiException`. Для разработки: API в одном терминале (`dotnet run --project ZAMETKI.Api`), клиент — в другом (`dotnet build ZAMETKI -t:Run -f net9.0-windows10.0.19041.0`) или через VS.

## Ссылки

- [@ТЗ_Онлайн-заметки_MVP.md](ТЗ_Онлайн-заметки_MVP.md) — полное ТЗ: роли, фичи MVP, модель данных (USER, GROUP, GROUP_MEMBER, NOTE), сценарии. **Читать перед** работой над авторизацией, ролями, группами, заметками, рассылками.
