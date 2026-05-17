using ZAMETKI.ViewModels;

namespace ZAMETKI.Pages;

public partial class FeedPage : ContentPage
{
    private readonly FeedViewModel _vm;

    public FeedPage(FeedViewModel vm)
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
}
