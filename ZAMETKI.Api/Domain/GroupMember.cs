namespace ZAMETKI.Api.Domain;

public class GroupMember
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string GroupId { get; set; } = string.Empty;
    public string StudentFullName { get; set; } = string.Empty;
    public string? StudentId { get; set; }
    public string Status { get; set; } = GroupMemberStatus.Invited;
}

public static class GroupMemberStatus
{
    public const string Invited = "Invited";
    public const string Active = "Active";
}
