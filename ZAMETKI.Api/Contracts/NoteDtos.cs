namespace ZAMETKI.Api.Contracts;

public record NoteDto(
    string Id,
    string Title,
    string Content,
    string OwnerId,
    string NoteType,
    string? TargetGroupId,
    DateTime CreatedDate,
    DateTime ModifiedDate);

public record CreateNoteRequest(string Title, string Content);

public record UpdateNoteRequest(string Title, string Content);
