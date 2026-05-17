using ZAMETKI.ViewModels;

namespace ZAMETKI.Pages;

[QueryProperty(nameof(GroupId), "id")]
public partial class GroupDetailsPage : ContentPage
{
    private readonly GroupDetailsViewModel _vm;

    public GroupDetailsPage(GroupDetailsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    public string? GroupId { get; set; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrEmpty(GroupId) && _vm.GroupId != GroupId)
            await _vm.LoadAsync(GroupId);
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
