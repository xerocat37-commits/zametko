using ZAMETKI.Api.Contracts;
using ZAMETKI.ViewModels;

namespace ZAMETKI.Pages;

public partial class MyGroupsPage : ContentPage
{
    private readonly MyGroupsViewModel _vm;

    public MyGroupsPage(MyGroupsViewModel vm)
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

    private async void RefreshView_Refreshing(object sender, EventArgs e) => await _vm.LoadAsync();

    private async void AddGroupButton_Clicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Новая группа", "Название группы (специальность):", placeholder: "Например, ИВТ-21", maxLength: 100);
        if (string.IsNullOrWhiteSpace(name)) return;
        await _vm.CreateAsync(name.Trim());
    }

    private async void GroupsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not GroupDto group) return;
        ((CollectionView)sender).SelectedItem = null;
        await Shell.Current.GoToAsync($"groupDetails?id={group.Id}");
    }
}
