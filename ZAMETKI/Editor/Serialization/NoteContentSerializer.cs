using System.Text.Json;
using ZAMETKI.Editor.Models;

namespace ZAMETKI.Editor.Serialization;

public static class NoteContentSerializer
{
    private const int CurrentVersion = 1;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private sealed record Envelope(int Version, List<BlockBase> Blocks);

    public static string Serialize(IEnumerable<BlockBase> blocks)
        => JsonSerializer.Serialize(new Envelope(CurrentVersion, blocks.ToList()), Options);

    public static List<BlockBase> Deserialize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new List<BlockBase> { new ParagraphBlock() };

        var trimmed = raw.TrimStart();
        if (!trimmed.StartsWith('{'))
            return new List<BlockBase> { new ParagraphBlock { Text = raw } };

        try
        {
            var env = JsonSerializer.Deserialize<Envelope>(raw, Options);
            if (env?.Blocks is { Count: > 0 } list) return list;
        }
        catch (JsonException)
        {
            // битый JSON — не теряем содержимое
        }
        return new List<BlockBase> { new ParagraphBlock { Text = raw } };
    }

    public static string Preview(string? raw, int maxLen = 140)
    {
        var blocks = Deserialize(raw);
        var parts = blocks.Select(b => b switch
        {
            HeadingBlock h => h.Text,
            ParagraphBlock p => p.Text,
            FileBlock f => $"📎 {f.FileName}",
            _ => string.Empty
        }).Where(s => !string.IsNullOrWhiteSpace(s));

        var joined = string.Join(" · ", parts);
        return joined.Length > maxLen ? joined[..maxLen] + "…" : joined;
    }
}
