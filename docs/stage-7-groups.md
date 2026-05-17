# Этап 7 — Клиент: группы препода (создание, приглашения, групповые заметки)

**Дата:** 2026-05-18
**Цель:** препод выполняет все свои сценарии из ТЗ — создаёт группу, приглашает студентов, ведёт групповые заметки.

## Что создано

### API-обёртки
- [Services/GroupsApi.cs](../ZAMETKI/Services/GroupsApi.cs) — `GetMyAsync`, `CreateAsync(name)`, `GetByIdAsync(id)`.
- [Services/GroupMembersApi.cs](../ZAMETKI/Services/GroupMembersApi.cs) — `ListAsync(groupId)`, `InviteAsync(groupId, fullName)`.
- [Services/GroupNotesApi.cs](../ZAMETKI/Services/GroupNotesApi.cs) — `ListAsync(groupId)`, `CreateAsync(groupId, title, content)`.

### ViewModels
- [ViewModels/MyGroupsViewModel.cs](../ZAMETKI/ViewModels/MyGroupsViewModel.cs) — список своих групп, `CreateAsync(name)` (создаёт через API и вставляет в коллекцию).
- [ViewModels/GroupDetailsViewModel.cs](../ZAMETKI/ViewModels/GroupDetailsViewModel.cs) — `GroupId`, `GroupName`, две коллекции (`Members`, `Notes`), методы `LoadAsync(id)`, `InviteAsync`, `CreateNoteAsync`.
- [ViewModels/GroupNoteEditorViewModel.cs](../ZAMETKI/ViewModels/GroupNoteEditorViewModel.cs) — `SaveCommand` → POST → `Shell.Current.GoToAsync("..")` (возврат к деталям).

### Страницы
- [Pages/MyGroupsPage.xaml](../ZAMETKI/Pages/MyGroupsPage.xaml) (+`.cs`) — переписана с заглушки. `RefreshView` + `CollectionView` со списком групп, кнопка «+ Новая группа» через `DisplayPromptAsync`. Тап по группе → `Shell.Current.GoToAsync($"groupDetails?id={group.Id}")`.
- [Pages/GroupDetailsPage.xaml](../ZAMETKI/Pages/GroupDetailsPage.xaml) (+`.cs`) — две секции: «Студенты» (ФИО + бейдж `Status` Invited/Active) и «Групповые заметки». Кнопки «+ Пригласить студента» (DisplayPrompt) и «+ Новая заметка» (переход в `groupNoteEditor`). Маршрут принимает `?id=` через `[QueryProperty]`.
- [Pages/GroupNoteEditorPage.xaml](../ZAMETKI/Pages/GroupNoteEditorPage.xaml) (+`.cs`) — `Entry` для заголовка + `Editor` для контента, кнопка «Сохранить». Принимает `?groupId=`.

### Маршруты
В [AppShell.xaml.cs](../ZAMETKI/AppShell.xaml.cs) добавлены:
- `groupDetails` → `GroupDetailsPage`
- `groupNoteEditor` → `GroupNoteEditorPage`

### DI
В [MauiProgram.cs](../ZAMETKI/MauiProgram.cs) — три новых API-singleton'а, три VM и три страницы (Transient).

## Архитектурные решения

- **`[QueryProperty]` для передачи `id`.** Стандартный механизм Shell для параметризованной навигации. На клиенте — атрибут на странице; параметр сетится через `?id={groupId}` в URL.
- **`OnAppearing` загружает данные, если `vm.GroupId != Id`.** Защита от повторной загрузки при возврате на страницу из дочернего экрана редактирования заметки.
- **Возврат после создания заметки — `GoToAsync("..")`.** Двойная точка = pop из стека Shell, возвращает к деталям группы. Список заметок при возврате перезагружается через `OnAppearing` (если изменился `GroupId`).
- **Приглашение через `DisplayPromptAsync`, не отдельная страница.** Одно поле (ФИО) не оправдывает целую страницу с VM.
- **`MyGroupsViewModel.CreateAsync` вставляет в начало коллекции** через `Insert(0, group)` — не нужно перегружать весь список после успешного создания. Сервер уже вернул свежий `GroupDto`.

## Проверка

```bash
dotnet build ZAMETKI/ZAMETKI.csproj -f net9.0-windows10.0.19041.0
```

Результат: 0 ошибок, 27 предупреждений MVVMTK0045 (только AOT-рекомендации).

### Сценарий пользователя
1. Логинимся как препод → во Flyout появляется «Мои группы».
2. Жмём «+ Новая группа» → вводим «ИВТ-21» → группа появляется в списке.
3. Тапаем по группе → детали: пустые секции «Студенты» и «Групповые заметки».
4. «+ Пригласить студента» → вводим «Петров П.П.» → в секции «Студенты» появляется с бейджем `Invited`.
5. «+ Новая заметка» → редактор → заголовок «Лекция 5», содержимое → «Сохранить» → возврат на детали, заметка в списке.
6. Студент Петров регистрируется с группой `ИВТ-21` и ФИО `Петров П.П.` → 200 → попадает в `main`.
7. Возвращаемся к преподу, обновляем детали группы → Петров теперь `Active`.
8. У студента в Ленте — групповая заметка «Лекция 5».

## Результат

Препод выполняет полный набор операций из ТЗ. На клиенте остаётся одна непокрытая роль — Head с его рассылками (этап 8).
