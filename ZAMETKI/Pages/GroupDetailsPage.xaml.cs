using ZAMETKI.ViewModels;

namespace ZAMETKI.Pages;

[QueryProperty(nameof(Id), "id")]
public partial class GroupDetailsPage : ContentPage
{
    private readonly GroupDetailsViewModel _vm;

    public GroupDetailsPage(GroupDetailsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    public string? Id { get; set; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrEmpty(Id) && _vm.GroupId != Id)
            await _vm.LoadAsync(Id);
    }

    private async void InviteButton_Clicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Пригласить студента", "ФИО студента:", placeholder: "Иванов И.И.", maxLength: 200);
        if (string.IsNullOrWhiteSpace(name)) return;
        await _vm.InviteAsync(name.Trim());
    }

    private async void NewNoteButton_Clicked(object sender, EventArgs e)
    {
        if (_vm.GroupId is null) return;
        await Shell.Current.GoToAsync($"groupNoteEditor?groupId={_vm.GroupId}");
    }
}
