using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ZAMETKI.Editor.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ParagraphBlock), "paragraph")]
[JsonDerivedType(typeof(HeadingBlock), "heading")]
[JsonDerivedType(typeof(FileBlock), "file")]
public abstract partial class BlockBase : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public event EventHandler? FocusRequested;

    public void RequestFocus() => FocusRequested?.Invoke(this, EventArgs.Empty);
}
