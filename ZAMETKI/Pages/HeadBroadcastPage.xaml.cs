using ZAMETKI.ViewModels;

namespace ZAMETKI.Pages;

public partial class HeadBroadcastPage : ContentPage
{
    private readonly HeadBroadcastViewModel _vm;

    public HeadBroadcastPage(HeadBroadcastViewModel vm)
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
}
