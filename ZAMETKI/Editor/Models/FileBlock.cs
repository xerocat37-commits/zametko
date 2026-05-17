using CommunityToolkit.Mvvm.ComponentModel;

namespace ZAMETKI.Editor.Models;

public partial class FileBlock : BlockBase
{
    [ObservableProperty] private string fileName = string.Empty;
    [ObservableProperty] private string contentType = string.Empty;
    [ObservableProperty] private long sizeBytes;
    [ObservableProperty] private string? localPath;
}
