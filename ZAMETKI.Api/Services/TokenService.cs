using Microsoft.EntityFrameworkCore;
using ZAMETKI.Api.Data;
using ZAMETKI.Api.Domain;

namespace ZAMETKI.Api.Services;

public class TokenService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(30);

    private readonly AppDbContext _db;

    public TokenService(AppDbContext db) => _db = db;

    public async Task<Session> CreateSessionAsync(string userId)
    {
        var session = new Session
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.Add(Ttl)
        };
        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();
        return session;
    }

    public async Task<User?> GetUserBySessionAsync(string token)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == token);
        if (session is null || session.ExpiresAt <= DateTime.UtcNow) return null;
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == session.UserId);
    }

    public async Task DeleteSessionAsync(string token)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == token);
        if (session is null) return;
        _db.Sessions.Remove(session);
        await _db.SaveChangesAsync();
    }
}
