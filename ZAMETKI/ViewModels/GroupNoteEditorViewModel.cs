using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZAMETKI.Api;
using ZAMETKI.Services;

namespace ZAMETKI.ViewModels;

public partial class GroupNoteEditorViewModel : ObservableObject
{
    private readonly GroupNotesApi _notes;

    public GroupNoteEditorViewModel(GroupNotesApi notes) => _notes = notes;

    [ObservableProperty] private string? groupId;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string content = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;
    [ObservableProperty] private string? errorMessage;

    public bool IsNotBusy => !IsBusy;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;
        if (string.IsNullOrWhiteSpace(GroupId) || string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Заголовок не может быть пустым.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await _notes.CreateAsync(GroupId, Title.Trim(), Content ?? string.Empty);
            await Shell.Current.GoToAsync("..");
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
