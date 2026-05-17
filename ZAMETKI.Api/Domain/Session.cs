namespace ZAMETKI.Api.Domain;

public class Session
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
