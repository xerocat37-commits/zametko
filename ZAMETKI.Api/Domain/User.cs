namespace ZAMETKI.Api.Domain;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FullName { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public string Role { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

public static class UserRoles
{
    public const string Student = "Student";
    public const string Teacher = "Teacher";
    public const string Head = "Head";
}
