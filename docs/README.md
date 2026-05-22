# Отчёты по этапам разработки MVP

Хронологический журнал создания MVP проекта **ZAMETKI**. Каждый этап — отдельный отчёт с целью, изменёнными файлами, архитектурными решениями и проверкой.

## Указатель

### Серверная часть
1. [Этап 1 — Серверный скелет + модель данных + миграция](stage-1-server-skeleton.md) — создание `ZAMETKI.Api`, EF Core, 5 таблиц.
2. [Этап 2 — Аутентификация: seed Head + register/login/logout/me + Bearer middleware](stage-2-auth.md) — `AuthController`, BCrypt, сессии в БД.
3. [Этап 3 — Бэкенд: Notes, Groups, GroupMembers, Group/Global notes](stage-3-backend-full.md) — все 13 эндпоинтов API, RoleGuard.

### Клиентская часть
4. [Этап 4 — Клиент: HttpClient, ApiClient, AuthService, DI, реструктуризация](stage-4-client-infrastructure.md) — инфраструктура без UI.
5. [Этап 5 — Клиент: Login / Register / RoleSelection + загрузочный gate](stage-5-auth-pages.md) — экраны входа и регистрации.
6. [Этап 6 — Клиент: замена Personal-заметок на API + AppShell с ролевой навигацией](stage-6-notes-api-shell.md) — Flyout, удаление локальной SQLite.
7. [Этап 7 — Клиент: группы препода (создание, приглашения, групповые заметки)](stage-7-groups.md) — функционал Teacher.
8. [Этап 8 — Клиент: панель Заведующего (рассылки) + финальная чистка](stage-8-broadcasts.md) — функционал Head, обновление CLAUDE.md.

## Запуск MVP

В двух терминалах:

```bash
# Терминал 1 — API
dotnet run --project ZAMETKI.Api/ZAMETKI.Api.csproj
# Слушает http://localhost:5000

# Терминал 2 — клиент (Windows)
dotnet build ZAMETKI/ZAMETKI.csproj -t:Run -f net9.0-windows10.0.19041.0
```

Учётные данные seed-Head: `Заведующий` / `head-admin` (меняются в `ZAMETKI.Api/appsettings.json`).
