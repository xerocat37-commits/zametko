using ZAMETKI.ViewModels;

namespace ZAMETKI.Pages;

[QueryProperty(nameof(NoteId), "noteId")]
public partial class PersonalNoteEditorPage : ContentPage
{
    private readonly PersonalNoteEditorViewModel _vm;

    public PersonalNoteEditorPage(PersonalNoteEditorViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    public string? NoteId
    {
        set => _vm.NoteId = string.IsNullOrEmpty(value) ? null : Uri.UnescapeDataString(value);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
