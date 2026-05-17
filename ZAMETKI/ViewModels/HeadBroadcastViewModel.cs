using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZAMETKI.Api;
using ZAMETKI.Api.Contracts;
using ZAMETKI.Services;

namespace ZAMETKI.ViewModels;

public partial class HeadBroadcastViewModel : ObservableObject
{
    private readonly GlobalNotesApi _global;
    private readonly NotesApi _notes;

    public HeadBroadcastViewModel(GlobalNotesApi global, NotesApi notes)
    {
        _global = global;
        _notes = notes;
    }

    public ObservableCollection<NoteDto> Broadcasts { get; } = new();

    [ObservableProperty] private string newTitle = string.Empty;
    [ObservableProperty] private string newContent = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;
    [ObservableProperty] private string? errorMessage;

    public bool IsNotBusy => !IsBusy;

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var feed = await _notes.GetFeedAsync();
            Broadcasts.Clear();
            foreach (var n in (feed ?? new List<NoteDto>()).Where(n => n.NoteType == NoteTypes.Global))
                Broadcasts.Add(n);
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

    [RelayCommand]
    private async Task CreateAsync()
    {
        if (IsBusy) return;
        if (string.IsNullOrWhiteSpace(NewTitle))
        {
            ErrorMessage = "Заголовок не может быть пустым.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var note = await _global.CreateAsync(NewTitle.Trim(), NewContent ?? string.Empty);
            Broadcasts.Insert(0, note);
            NewTitle = string.Empty;
            NewContent = string.Empty;
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Сервер недоступен: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
