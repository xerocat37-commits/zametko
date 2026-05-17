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
        await Shell.Current.GoToAsync("personalNoteEditor");
    }

    private async void NotesCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not NoteDto note) return;
        NotesCollectionView.SelectedItem = null;

        await Shell.Current.GoToAsync($"personalNoteEditor?noteId={Uri.EscapeDataString(note.Id)}");
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
