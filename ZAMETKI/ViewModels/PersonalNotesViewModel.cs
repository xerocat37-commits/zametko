using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZAMETKI.Api;
using ZAMETKI.Api.Contracts;
using ZAMETKI.Services;

namespace ZAMETKI.ViewModels;

public partial class PersonalNotesViewModel : ObservableObject
{
    private readonly NotesApi _notes;

    private List<NoteDto> _all = new();

    public PersonalNotesViewModel(NotesApi notes) => _notes = notes;

    public ObservableCollection<NoteDto> Notes { get; } = new();

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var items = await _notes.GetMyAsync();
            _all = items ?? new List<NoteDto>();
            ApplyFilter();
        }
        catch (ApiException ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Сервер недоступен: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> CreateAsync(string title, string content)
    {
        try
        {
            var note = await _notes.CreateAsync(title, content);
            _all.Insert(0, note);
            ApplyFilter();
            return true;
        }
        catch (ApiException ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(NoteDto note, string title, string content)
    {
        try
        {
            var updated = await _notes.UpdateAsync(note.Id, title, content);
            var idx = _all.FindIndex(n => n.Id == note.Id);
            if (idx >= 0) _all[idx] = updated;
            ApplyFilter();
            return true;
        }
        catch (ApiException ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(NoteDto note)
    {
        try
        {
            await _notes.DeleteAsync(note.Id);
            _all.RemoveAll(n => n.Id == note.Id);
            ApplyFilter();
            return true;
        }
        catch (ApiException ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            return false;
        }
    }

    private void ApplyFilter()
    {
        var query = SearchText?.Trim() ?? string.Empty;
        IEnumerable<NoteDto> filtered = _all;
        if (!string.IsNullOrEmpty(query))
        {
            filtered = _all.Where(n =>
                (n.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (n.Content?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        Notes.Clear();
        foreach (var n in filtered) Notes.Add(n);
    }
}
