using CommunityToolkit.Mvvm.ComponentModel;

namespace ZAMETKI.Editor.Models;

public partial class ParagraphBlock : BlockBase
{
    [ObservableProperty] private string text = string.Empty;
}
