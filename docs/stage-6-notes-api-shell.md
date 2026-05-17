# Этап 6 — Клиент: замена Personal-заметок на API + AppShell с ролевой навигацией

**Дата:** 2026-05-17
**Цель:** локальная SQLite выпиливается; Personal-заметки ходят на API; AppShell показывает разные вкладки по роли.

## Что создано

### Сервис и VM
- [Services/NotesApi.cs](../ZAMETKI/Services/NotesApi.cs) — `GetMyAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `GetFeedAsync`.
- [ViewModels/PersonalNotesViewModel.cs](../ZAMETKI/ViewModels/PersonalNotesViewModel.cs) — `ObservableCollection<NoteDto> Notes`, поиск в памяти через `SearchText` + `ApplyFilter()` (без запросов к серверу при изменении строки поиска), методы `LoadAsync`/`CreateAsync`/`UpdateAsync`/`DeleteAsync`. Ошибки от API показывает через `Shell.Current.DisplayAlert`.
- [ViewModels/FeedViewModel.cs](../ZAMETKI/ViewModels/FeedViewModel.cs) — read-only лента, `LoadAsync` через `/api/notes/feed`.

### Страницы
- [Pages/PersonalNotesPage.xaml](../ZAMETKI/Pages/PersonalNotesPage.xaml) (+`.cs`) — тот же UI, что в старом MainPage (SearchBar + CollectionView + SwipeView), но `ItemsSource` биндится к `Notes`, источник — `NotesApi`.
- [Pages/FeedPage.xaml](../ZAMETKI/Pages/FeedPage.xaml) (+`.cs`) — карточки с бейджем типа (`NoteType` цветным шрифтом), `RefreshView` для pull-to-refresh.
- [Pages/MyGroupsPage.xaml](../ZAMETKI/Pages/MyGroupsPage.xaml) — заглушка «в разработке» (наполнится в этапе 7).
- [Pages/HeadBroadcastPage.xaml](../ZAMETKI/Pages/HeadBroadcastPage.xaml) — заглушка «в разработке» (наполнится в этапе 8).

### Навигация (Flyout + роли)
- [AppShell.xaml](../ZAMETKI/AppShell.xaml) — root `ShellContent` для `login`; `FlyoutItem` «Главная» с `FlyoutDisplayOptions="AsMultipleItems"` и четырьмя `ShellContent`'ами: Личные, Лента, Мои группы (`x:Name="MyGroupsShellItem"`), Рассылка (`x:Name="HeadBroadcastShellItem"`). В `Shell.FlyoutFooter` — кнопка «Выйти».
- [AppShell.xaml.cs](../ZAMETKI/AppShell.xaml.cs) — конструктор принимает `AuthService`. Подписан на `AuthStateChanged`; `ApplyRoleVisibility()`:
  - `MyGroupsShellItem.IsVisible = role == Teacher`
  - `HeadBroadcastShellItem.IsVisible = role == Head`
  - `FlyoutBehavior` переключается между `Flyout` (вход выполнен) и `Disabled` (на экране Login).
- [App.xaml.cs](../ZAMETKI/App.xaml.cs) — теперь резолвит `AppShell` через DI.

### Чистка (-)
- Удалены файлы:
  - `ZAMETKI/Models/Note.cs` (старая модель с `int Id`).
  - `ZAMETKI/Services/NoteService.cs` (локальный SQLite-CRUD).
  - `ZAMETKI/MainPage.xaml`, `ZAMETKI/MainPage.xaml.cs`.
  - Папка `ZAMETKI/Models/` исчезла.
- Из [ZAMETKI/ZAMETKI.csproj](../ZAMETKI/ZAMETKI.csproj) убраны:
  - `sqlite-net-pcl` 1.9.172
  - `SQLitePCLRaw.bundle_green` 2.1.8

### DI
В [MauiProgram.cs](../ZAMETKI/MauiProgram.cs) добавлены: `NotesApi` и `AppShell` (Singleton); `PersonalNotesViewModel`, `FeedViewModel`, `PersonalNotesPage`, `FeedPage`, `MyGroupsPage`, `HeadBroadcastPage` (Transient).

## Архитектурные решения

- **Поиск на клиенте, а не на сервере.** API не предоставляет `?q=` параметр для `/api/notes`. Загружаем все Personal один раз и фильтруем в памяти (`Contains(query, OrdinalIgnoreCase)`). Для учебного MVP с десятками заметок этого достаточно; гонять запрос на каждое нажатие клавиши было бы расточительно.
- **`AppShell` — Singleton + получает `AuthService` через DI.** `App.xaml.cs` теперь резолвит `AppShell` через сервис-провайдер. Singleton — потому что Shell живёт всю сессию приложения; подписка на `AuthStateChanged` не должна теряться.
- **Заглушки `MyGroupsPage` / `HeadBroadcastPage`.** Чтобы не плодить условный код в AppShell, страницы есть в навигации сразу. На этапах 7–8 заменяются с тем же типом — переписывать XAML/cs, не Shell.
- **`RefreshView` в Feed, но не в Personal.** В Personal обновление происходит сразу после Create/Update/Delete (модификация коллекции в VM). В Feed данные приходят извне (от других пользователей) — нужен ручной pull-to-refresh.

## Проверка

```bash
dotnet build ZAMETKI/ZAMETKI.csproj -f net9.0-windows10.0.19041.0
```

Результат: 0 ошибок, 16 предупреждений MVVMTK0045 (только AOT-рекомендации). XC0022 на MainPage исчезли — файл удалён.

### Что должно работать
- Personal-заметки создаются/редактируются/удаляются через API. Открыли клиент на втором устройстве под тем же юзером — те же заметки.
- Лента у студента: Global рассылки + Group-заметки своих групп.
- Лента у препода: Global + свои Group-заметки.
- Лента у Head: только Global.
- Во Flyout у Teacher есть «Мои группы», у Head есть «Рассылка», у Student — нет ни того, ни другого.
- Кнопка «Выйти» в Flyout: чистит SecureStorage, ведёт на `//login`.

## Результат

Локальная SQLite полностью выпилена. Клиент работает строго через API. Ролевая навигация во Flyout управляется одним свойством `User.Role`. Заглушки готовы к замене.
