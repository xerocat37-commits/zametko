namespace ZAMETKI.Api.Contracts;

public record GroupDto(string Id, string Name, string TeacherId, DateTime CreatedDate);

public record CreateGroupRequest(string Name);

public record GroupMemberDto(string Id, string GroupId, string StudentFullName, string? StudentId, string Status);

public record InviteStudentRequest(string StudentFullName);

public static class UserRoles
{
    public const string Student = "Student";
    public const string Teacher = "Teacher";
    public const string Head = "Head";
}

public static class GroupMemberStatus
{
    public const string Invited = "Invited";
    public const string Active = "Active";
}
