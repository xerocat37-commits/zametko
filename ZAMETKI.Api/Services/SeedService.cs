using Microsoft.EntityFrameworkCore;
using ZAMETKI.Api.Data;
using ZAMETKI.Api.Domain;

namespace ZAMETKI.Api.Services;

public class SeedService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher _hasher;
    private readonly IConfiguration _config;
    private readonly ILogger<SeedService> _log;

    public SeedService(AppDbContext db, PasswordHasher hasher, IConfiguration config, ILogger<SeedService> log)
    {
        _db = db;
        _hasher = hasher;
        _config = config;
        _log = log;
    }

    public async Task SeedAsync()
    {
        if (await _db.Users.AnyAsync(u => u.Role == UserRoles.Head)) return;

        var fullName = _config["HeadAdmin:FullName"];
        var password = _config["HeadAdmin:Password"];
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(password))
        {
            _log.LogWarning("HeadAdmin config is missing; no Head will be seeded.");
            return;
        }

        var head = new User
        {
            FullName = fullName,
            Role = UserRoles.Head,
            PasswordHash = _hasher.Hash(password),
            CreatedDate = DateTime.UtcNow
        };
        _db.Users.Add(head);
        await _db.SaveChangesAsync();
        _log.LogInformation("Seeded Head admin: {FullName}", fullName);
    }
}
