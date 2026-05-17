using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZAMETKI.Api;
using ZAMETKI.Api.Contracts;
using ZAMETKI.Services;

namespace ZAMETKI.ViewModels;

public partial class PersonalNoteEditorViewModel : ObservableObject
{
    private readonly NotesApi _notes;

    public NoteEditorViewModel Editor { get; }

    public PersonalNoteEditorViewModel(NotesApi notes, NoteEditorViewModel editor)
    {
        _notes = notes;
        Editor = editor;
        Editor.Load(null);
    }

    [ObservableProperty] private string? noteId;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;
    [ObservableProperty] private string? errorMessage;

    public bool IsNotBusy => !IsBusy;

    public bool IsNew => string.IsNullOrEmpty(NoteId);

    public async Task LoadAsync()
    {
        if (string.IsNullOrEmpty(NoteId))
        {
            Title = string.Empty;
            Editor.Load(null);
            return;
        }

        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var list = await _notes.GetMyAsync();
            var note = list?.FirstOrDefault(n => n.Id == NoteId);
            if (note is null)
            {
                ErrorMessage = "Заметка не найдена.";
                return;
            }
            Title = note.Title;
            Editor.Load(note.Content);
        }
        catch (ApiException ex) { ErrorMessage = ex.Message; }
        catch (Exception ex) { ErrorMessage = $"Сервер недоступен: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;
        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Заголовок не может быть пустым.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var content = Editor.Serialize();
            if (IsNew)
                await _notes.CreateAsync(Title.Trim(), content);
            else
                await _notes.UpdateAsync(NoteId!, Title.Trim(), content);
            await Shell.Current.GoToAsync("..");
        }
        catch (ApiException ex) { ErrorMessage = ex.Message; }
        catch (Exception ex) { ErrorMessage = $"Сервер недоступен: {ex.Message}"; }
        finally { IsBusy = false; }
    }
}
