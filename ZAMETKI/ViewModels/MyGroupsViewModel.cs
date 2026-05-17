using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ZAMETKI.Api;
using ZAMETKI.Api.Contracts;
using ZAMETKI.Services;

namespace ZAMETKI.ViewModels;

public partial class MyGroupsViewModel : ObservableObject
{
    private readonly GroupsApi _groups;

    public MyGroupsViewModel(GroupsApi groups) => _groups = groups;

    public ObservableCollection<GroupDto> Groups { get; } = new();

    [ObservableProperty] private bool isBusy;

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var items = await _groups.GetMyAsync();
            Groups.Clear();
            foreach (var g in items ?? new List<GroupDto>()) Groups.Add(g);
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

    public async Task<bool> CreateAsync(string name)
    {
        try
        {
            var group = await _groups.CreateAsync(name);
            Groups.Insert(0, group);
            return true;
        }
        catch (ApiException ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            return false;
        }
    }
}
