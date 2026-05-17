using ZAMETKI.Api.Contracts;
using ZAMETKI.ViewModels;

namespace ZAMETKI.Pages;

public partial class PersonalNotesPage : ContentPage
{
    private readonly PersonalNotesViewModel _vm;

    public PersonalNotesPage(PersonalNotesViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    private async void AddNoteButton_Clicked(object sender, EventArgs e)
    {
        var title = await DisplayPromptAsync("Новая заметка", "Заголовок:", placeholder: "Заголовок", maxLength: 100);
        if (string.IsNullOrWhiteSpace(title)) return;

        var content = await DisplayPromptAsync("Новая заметка", "Текст:", placeholder: "Текст заметки...", maxLength: 5000);
        if (content is null) return;

        await _vm.CreateAsync(title.Trim(), content);
    }

    private async void NotesCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not NoteDto note) return;
        NotesCollectionView.SelectedItem = null;

        var action = await DisplayActionSheet(note.Title, "Отмена", "Удалить", "Редактировать заголовок", "Редактировать содержимое");
        switch (action)
        {
            case "Редактировать заголовок":
                var newTitle = await DisplayPromptAsync("Заголовок", "Новый заголовок:", initialValue: note.Title, maxLength: 100);
                if (!string.IsNullOrWhiteSpace(newTitle) && newTitle != note.Title)
                    await _vm.UpdateAsync(note, newTitle.Trim(), note.Content);
                break;
            case "Редактировать содержимое":
                var newContent = await DisplayPromptAsync("Содержимое", "Новый текст:", initialValue: note.Content, maxLength: 5000);
                if (newContent is not null && newContent != note.Content)
                    await _vm.UpdateAsync(note, note.Title, newContent);
                break;
            case "Удалить":
                if (await DisplayAlert("Подтверждение", $"Удалить '{note.Title}'?", "Удалить", "Отмена"))
                    await _vm.DeleteAsync(note);
                break;
        }
    }

    private async void DeleteSwipeItem_Invoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem item && item.BindingContext is NoteDto note)
        {
            if (await DisplayAlert("Подтверждение", $"Удалить '{note.Title}'?", "Удалить", "Отмена"))
                await _vm.DeleteAsync(note);
        }
    }
}
