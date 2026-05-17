using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZAMETKI.Api;
using ZAMETKI.Services;

namespace ZAMETKI.ViewModels;

public partial class RegisterTeacherViewModel : ObservableObject
{
    private readonly AuthService _auth;

    public RegisterTeacherViewModel(AuthService auth) => _auth = auth;

    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;
    [ObservableProperty] private string? errorMessage;

    public bool IsNotBusy => !IsBusy;

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Заполните ФИО и пароль.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await _auth.RegisterTeacherAsync(FullName.Trim(), Password);
            Password = string.Empty;
            await Shell.Current.GoToAsync("//main");
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
