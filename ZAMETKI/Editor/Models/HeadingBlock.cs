using CommunityToolkit.Mvvm.ComponentModel;

namespace ZAMETKI.Editor.Models;

public partial class HeadingBlock : BlockBase
{
    [ObservableProperty] private string text = string.Empty;

    // 1=H1 (28sp), 2=H2 (22sp), 3=H3 (18sp)
    [ObservableProperty] private int level = 1;
}
