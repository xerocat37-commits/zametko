namespace ZAMETKI.Api.Domain;

public class Note
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string NoteType { get; set; } = string.Empty;
    public string? TargetGroupId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
}

public static class NoteTypes
{
    public const string Personal = "Personal";
    public const string Group = "Group";
    public const string Global = "Global";
}
