namespace ZAMETKI.Api.Contracts;

public record GroupMemberDto(string Id, string GroupId, string StudentFullName, string? StudentId, string Status);

public record InviteStudentRequest(string StudentFullName);
