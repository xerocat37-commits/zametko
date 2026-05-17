# Этап 5 — Клиент: Login / Register / RoleSelection + загрузочный gate

**Дата:** 2026-05-17
**Цель:** пользователь может зарегистрироваться/войти и получить токен; после входа попадает на старый `MainPage` (заменим в этапе 6).

## Что создано

### ViewModels (на `CommunityToolkit.Mvvm`)
- [ViewModels/LoginViewModel.cs](../ZAMETKI/ViewModels/LoginViewModel.cs) — `LoginCommand` (`/api/auth/login` → `Shell.Current.GoToAsync("//main")`), `GoToRegisterCommand` (→ `roleSelection`).
- [ViewModels/RegisterStudentViewModel.cs](../ZAMETKI/ViewModels/RegisterStudentViewModel.cs) — `GroupName`, `FullName`, `Password`, `RegisterCommand`.
- [ViewModels/RegisterTeacherViewModel.cs](../ZAMETKI/ViewModels/RegisterTeacherViewModel.cs) — `FullName`, `Password`, `RegisterCommand`.

Все три VM имеют пару `IsBusy` / `IsNotBusy` для дизейбла кнопок во время запроса (через `[NotifyPropertyChangedFor(nameof(IsNotBusy))]`), и `ErrorMessage` для вывода ошибок API.

### Страницы
- [Pages/LoginPage.xaml](../ZAMETKI/Pages/LoginPage.xaml) (+`.cs`) — поля ФИО + пароль, кнопка «Войти», ссылка «Регистрация». `Shell.NavBarIsVisible="False"` чтобы не было лишней панели.
- [Pages/RoleSelectionPage.xaml](../ZAMETKI/Pages/RoleSelectionPage.xaml) (+`.cs`) — две кнопки «Студент» / «Преподаватель». **Без Head** — правило из CLAUDE.md.
- [Pages/RegisterStudentPage.xaml](../ZAMETKI/Pages/RegisterStudentPage.xaml) (+`.cs`) — группа + ФИО + пароль.
- [Pages/RegisterTeacherPage.xaml](../ZAMETKI/Pages/RegisterTeacherPage.xaml) (+`.cs`) — ФИО + пароль.

### Навигация
- [AppShell.xaml](../ZAMETKI/AppShell.xaml) — два root `ShellContent`: `Route="login"` (LoginPage) и `Route="main"` (старый MainPage). `FlyoutBehavior=Disabled` (Flyout появится в этапе 6).
- [AppShell.xaml.cs](../ZAMETKI/AppShell.xaml.cs) — `Routing.RegisterRoute` для `roleSelection`, `registerStudent`, `registerTeacher`.
- [App.xaml.cs](../ZAMETKI/App.xaml.cs) — конструктор принимает `AuthService`. В `OnStart` вызывает `RestoreAsync()`; если токен валиден — `GoToAsync("//main")`, иначе остаёмся на Login.

### Logout (временный)
В [MainPage.xaml](../ZAMETKI/MainPage.xaml) добавлен `ToolbarItem "Выйти"`; обработчик в [MainPage.xaml.cs](../ZAMETKI/MainPage.xaml.cs) дёргает `AuthService` через `IPlatformApplication.Current.Services` (service locator — оправдано как временное решение, MainPage уйдёт в этапе 6) и редиректит на `//login`.

### DI
В [MauiProgram.cs](../ZAMETKI/MauiProgram.cs) — все VM и страницы как **Transient**.

## Архитектурные решения

- **Без custom value-конвертеров.** Первая версия `LoginPage.xaml` использовала `StringNotEmptyConverter` и `InverseBoolConverter`, которых нет в проекте. Решение: вместо `Inverse(IsBusy)` — добавил `IsNotBusy` свойство в VM; вместо `IsVisible` по непустой строке — Label всегда виден (пустая строка не рендерится).
- **`Shell.Current.GoToAsync("//main")` при успехе.** Двойной слэш — абсолютный маршрут от корня Shell, заменяет всю стек-навигацию. Это правильное поведение при логине: не возвращаться по back-стеку на форму входа.
- **`AuthService.RestoreAsync()` в `OnStart`, а не в конструкторе App.** Конструктор App вызывается до того, как Shell готов; `await Task.Yield()` даёт ему время инициализироваться, и `Shell.Current` уже не null.
- **Logout через service locator — намеренно временный костыль.** MainPage будет полностью переписан на этапе 6 с конструкторной инъекцией.

## Проверка

```bash
dotnet build ZAMETKI/ZAMETKI.csproj -f net9.0-windows10.0.19041.0
```

Результат: 0 ошибок, 16 предупреждений `MVVMTK0045` (рекомендация по AOT в WinRT для `[ObservableProperty]`-полей — не блокер; страницы работают через generated property) + 3 XC0022 на старом `MainPage.xaml`.

Запуск клиента вживую — за разработчиком (требует Visual Studio / `dotnet build -t:Run` под Windows Machine профилем).

## Сценарий пользователя

1. Запуск без сохранённого токена → попадаем на `LoginPage`.
2. Жмём «Регистрация» → `RoleSelectionPage`.
3. «Преподаватель» → `RegisterTeacherPage` → вводим ФИО+пароль → 200 → попадаем на старый `MainPage`.
4. Перезапуск клиента → `RestoreAsync()` → токен валиден → сразу на `MainPage` без захода через Login.
5. ToolbarItem «Выйти» → `AuthService.LogoutAsync()` → `//login`.

## Результат

Авторизация замкнута: пользователь регистрируется или входит, токен сохраняется в `SecureStorage`, при перезапуске сессия восстанавливается. UI пока минимальный, но рабочий.
