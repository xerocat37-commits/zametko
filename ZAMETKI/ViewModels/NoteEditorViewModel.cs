using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZAMETKI.Editor.Models;
using ZAMETKI.Editor.Serialization;

namespace ZAMETKI.ViewModels;

public partial class NoteEditorViewModel : ObservableObject
{
    public ObservableCollection<BlockBase> Blocks { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditable))]
    private bool isReadOnly;

    public bool IsEditable => !IsReadOnly;

    public void Load(string? rawContent)
    {
        Blocks.Clear();
        foreach (var b in NoteContentSerializer.Deserialize(rawContent))
            Blocks.Add(b);
    }

    public string Serialize() => NoteContentSerializer.Serialize(Blocks);

    [RelayCommand]
    private void AddParagraph()
    {
        if (IsReadOnly) return;
        InsertAfter(Blocks.LastOrDefault(), new ParagraphBlock(), focus: true);
    }

    [RelayCommand]
    private void AddHeading()
    {
        if (IsReadOnly) return;
        InsertAfter(Blocks.LastOrDefault(), new HeadingBlock { Level = 2 }, focus: true);
    }

    [RelayCommand]
    private async Task AddFileAsync()
    {
        if (IsReadOnly) return;
        FileResult? pick = null;
        try
        {
            pick = await FilePicker.Default.PickAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Файл", $"Не удалось выбрать файл: {ex.Message}", "OK");
            return;
        }
        if (pick is null) return;

        InsertAfter(Blocks.LastOrDefault(), new FileBlock
        {
            FileName = pick.FileName,
            ContentType = pick.ContentType ?? string.Empty,
            LocalPath = pick.FullPath,
            SizeBytes = TryGetSize(pick.FullPath)
        }, focus: false);
    }

    [RelayCommand]
    private void EnterPressed(BlockBase? current)
    {
        if (IsReadOnly || current is null) return;
        var next = new ParagraphBlock();
        InsertAfter(current, next, focus: true);
    }

    [RelayCommand]
    private async Task SlashTypedAsync(BlockBase? current)
    {
        if (IsReadOnly || current is null) return;

        var choice = await Shell.Current.DisplayActionSheet(
            "Тип блока", "Отмена", null,
            "Текст", "Заголовок", "Файл");

        if (current is ParagraphBlock p && p.Text == "/")
            p.Text = string.Empty;

        switch (choice)
        {
            case "Заголовок":
                ReplaceBlock(current, new HeadingBlock { Level = 2 }, focus: true);
                break;
            case "Файл":
                await AddFileAsync();
                break;
            case "Текст":
                current.RequestFocus();
                break;
        }
    }

    [RelayCommand]
    private void RemoveBlock(BlockBase? block)
    {
        if (IsReadOnly || block is null) return;
        var idx = Blocks.IndexOf(block);
        if (idx < 0) return;
        Blocks.RemoveAt(idx);
        if (Blocks.Count == 0)
            Blocks.Add(new ParagraphBlock());
        var focusTarget = idx >= Blocks.Count ? Blocks[^1] : Blocks[idx];
        focusTarget.RequestFocus();
    }

    [RelayCommand]
    private void MoveUp(BlockBase? b) => Move(b, -1);

    [RelayCommand]
    private void MoveDown(BlockBase? b) => Move(b, +1);

    [RelayCommand]
    private void CycleHeadingLevel(HeadingBlock? h)
    {
        if (IsReadOnly || h is null) return;
        h.Level = h.Level == 3 ? 1 : h.Level + 1;
    }

    [RelayCommand]
    private async Task OpenFileAsync(FileBlock? f)
    {
        if (f is null) return;
        if (string.IsNullOrWhiteSpace(f.LocalPath) || !File.Exists(f.LocalPath))
        {
            await Shell.Current.DisplayAlert("Файл", "Файл недоступен на этом устройстве.", "OK");
            return;
        }
        try
        {
            await Launcher.Default.OpenAsync(new OpenFileRequest(f.FileName, new ReadOnlyFile(f.LocalPath)));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Файл", ex.Message, "OK");
        }
    }

    private void InsertAfter(BlockBase? anchor, BlockBase newBlock, bool focus)
    {
        var idx = anchor is null ? Blocks.Count : Blocks.IndexOf(anchor) + 1;
        if (idx < 0) idx = Blocks.Count;
        Blocks.Insert(idx, newBlock);
        if (focus) newBlock.RequestFocus();
    }

    private void ReplaceBlock(BlockBase oldBlock, BlockBase replacement, bool focus)
    {
        var idx = Blocks.IndexOf(oldBlock);
        if (idx < 0) return;
        Blocks[idx] = replacement;
        if (focus) replacement.RequestFocus();
    }

    private void Move(BlockBase? b, int delta)
    {
        if (IsReadOnly || b is null) return;
        var idx = Blocks.IndexOf(b);
        var target = idx + delta;
        if (idx < 0 || target < 0 || target >= Blocks.Count) return;
        Blocks.Move(idx, target);
    }

    private static long TryGetSize(string? path)
    {
        try
        {
            return path is not null && File.Exists(path) ? new FileInfo(path).Length : 0;
        }
        catch
        {
            return 0;
        }
    }
}
