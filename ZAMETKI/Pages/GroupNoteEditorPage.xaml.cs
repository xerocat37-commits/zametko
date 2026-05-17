using ZAMETKI.ViewModels;

namespace ZAMETKI.Pages;

[QueryProperty(nameof(GroupId), "groupId")]
public partial class GroupNoteEditorPage : ContentPage
{
    private readonly GroupNoteEditorViewModel _vm;

    public GroupNoteEditorPage(GroupNoteEditorViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    public string? GroupId
    {
        set => _vm.GroupId = value;
    }
}
