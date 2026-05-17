# Этап 1 — Серверный скелет + модель данных + миграция

**Дата:** 2026-05-17
**Цель:** создать проект `ZAMETKI.Api` с пустыми контроллерами, EF Core, всеми доменными моделями и инициализированной БД.

## Что создано

### Проект и конфигурация
- [ZAMETKI.Api/ZAMETKI.Api.csproj](../ZAMETKI.Api/ZAMETKI.Api.csproj) — .NET 9 Web API (SDK `Microsoft.NET.Sdk.Web`), пакеты:
  - `Microsoft.EntityFrameworkCore.Sqlite` 9.0.0
  - `Microsoft.EntityFrameworkCore.Design` 9.0.0
  - `BCrypt.Net-Next` 4.0.3
- [ZAMETKI.Api/appsettings.json](../ZAMETKI.Api/appsettings.json) — `ConnectionStrings:Default` + секция `HeadAdmin` (для сидования заведующего на этапе 2).
- [ZAMETKI.Api/Properties/launchSettings.json](../ZAMETKI.Api/Properties/launchSettings.json) — оставлен только http-профиль на `http://localhost:5000`.
- Удалены сгенерированные шаблоном `WeatherForecast.cs`, `WeatherForecastController.cs`, `ZAMETKI.Api.http`.

### Доменные модели (по ТЗ)
- [Domain/User.cs](../ZAMETKI.Api/Domain/User.cs) — `Id (GUID PK)`, `FullName`, `GroupName?`, `Role`, `PasswordHash`, `CreatedDate`. Константы `UserRoles.Student/Teacher/Head`.
- [Domain/Group.cs](../ZAMETKI.Api/Domain/Group.cs) — `Id`, `Name`, `TeacherId`, `CreatedDate`.
- [Domain/GroupMember.cs](../ZAMETKI.Api/Domain/GroupMember.cs) — `Id`, `GroupId`, `StudentFullName`, `StudentId?`, `Status`. Константы `GroupMemberStatus.Invited/Active`.
- [Domain/Note.cs](../ZAMETKI.Api/Domain/Note.cs) — `Id`, `Title`, `Content`, `OwnerId`, `NoteType`, `TargetGroupId?`, `CreatedDate`, `ModifiedDate`. Константы `NoteTypes.Personal/Group/Global`.
- [Domain/Session.cs](../ZAMETKI.Api/Domain/Session.cs) — `Id (=Token)`, `UserId`, `ExpiresAt`.

### DbContext и миграция
- [Data/AppDbContext.cs](../ZAMETKI.Api/Data/AppDbContext.cs) — 5 DbSet'ов + индексы для частых запросов (`FullName`, `TeacherId`, `GroupId+StudentFullName` и т.д.).
- `Data/Migrations/20260517140324_Init.cs` (+ Designer + Snapshot) — создаёт все 5 таблиц.

### Program.cs
- [ZAMETKI.Api/Program.cs](../ZAMETKI.Api/Program.cs) — `WebApplication` с контроллерами, `AddDbContext<AppDbContext>`, привязка к `http://localhost:5000`. На старте: `db.Database.Migrate()` + `PRAGMA journal_mode=WAL;`.

### Solution и .gitignore
- `ZAMETKI.Api` добавлен в [ZAMETKI.sln](../ZAMETKI.sln) через `dotnet sln add`.
- В [.gitignore](../.gitignore) добавлены `ZAMETKI.Api/zametki.db*`.

## Архитектурные решения

- **Локально, без HTTPS.** Учебный MVP, один университет = одна машина. Не делать deploy-history-friendly self-signed cert.
- **EF Core + миграции.** Стандарт для ASP.NET Core, удобно показывать в дипломе. `Database.Migrate()` при старте — нет ручной шага «обнови БД».
- **WAL включён через `ExecuteSqlRaw`** после миграции, а не через `OnConfiguring` — это идемпотентно и видно в коде.
- **`Id` всех сущностей — `string` GUID.** Совпадает с ТЗ; убирает проблемы с числовыми ID при синхронизации с клиентом.
- **Без проекта `ZAMETKI.Shared`.** DTO будут дублироваться на стороне клиента и сервера — осознанный trade-off для учебного MVP.

## Проверка

```bash
dotnet build ZAMETKI.Api/ZAMETKI.Api.csproj   # 0 ошибок, 0 предупреждений
dotnet run --project ZAMETKI.Api               # слушает http://localhost:5000
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5000/   # 404 (контроллеров нет — ожидаемо)
```

В созданной БД `zametki.db` через `sqlite3 .tables`:
```
GroupMembers  Groups  Notes  Sessions  Users  __EFMigrationsHistory  __EFMigrationsLock
```

Файлы `zametki.db`, `zametki.db-shm`, `zametki.db-wal` — все на месте (WAL работает).

## Результат

Серверный скелет готов к подключению авторизации и контроллеров. На этом этапе ещё нет ни одного эндпоинта, поэтому пользовательских сценариев не проверяется — проверка только инфраструктурная.
