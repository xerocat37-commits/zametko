using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ZAMETKI.Api;
using ZAMETKI.Api.Contracts;
using ZAMETKI.Services;

namespace ZAMETKI.ViewModels;

public partial class FeedViewModel : ObservableObject
{
    private readonly NotesApi _notes;

    public FeedViewModel(NotesApi notes) => _notes = notes;

    public ObservableCollection<NoteDto> Feed { get; } = new();

    [ObservableProperty] private bool isBusy;

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var items = await _notes.GetFeedAsync();
            Feed.Clear();
            foreach (var n in items ?? new List<NoteDto>()) Feed.Add(n);
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
}
