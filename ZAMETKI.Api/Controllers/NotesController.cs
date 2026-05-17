using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZAMETKI.Api.Authorization;
using ZAMETKI.Api.Contracts;
using ZAMETKI.Api.Data;
using ZAMETKI.Api.Domain;

namespace ZAMETKI.Api.Controllers;

[ApiController]
[Route("api/notes")]
public class NotesController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetMyPersonal()
    {
        var (user, error) = RoleGuard.RequireAuth(HttpContext);
        if (error is not null) return error;

        var notes = await _db.Notes
            .Where(n => n.OwnerId == user!.Id && n.NoteType == NoteTypes.Personal)
            .OrderByDescending(n => n.ModifiedDate)
            .ToListAsync();
        return Ok(notes.Select(ToDto));
    }

    [HttpPost]
    public async Task<IActionResult> CreatePersonal([FromBody] CreateNoteRequest req)
    {
        var (user, error) = RoleGuard.RequireAuth(HttpContext);
        if (error is not null) return error;

        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest(new { error = "Заголовок не может быть пустым." });

        var now = DateTime.UtcNow;
        var note = new Note
        {
            Title = req.Title,
            Content = req.Content ?? string.Empty,
            OwnerId = user!.Id,
            NoteType = NoteTypes.Personal,
            CreatedDate = now,
            ModifiedDate = now
        };
        _db.Notes.Add(note);
        await _db.SaveChangesAsync();
        return Ok(ToDto(note));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePersonal(string id, [FromBody] UpdateNoteRequest req)
    {
        var (user, error) = RoleGuard.RequireAuth(HttpContext);
        if (error is not null) return error;

        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id);
        if (note is null) return NotFound();
        if (note.OwnerId != user!.Id || note.NoteType != NoteTypes.Personal) return RoleGuard.Forbidden();

        note.Title = req.Title;
        note.Content = req.Content ?? string.Empty;
        note.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ToDto(note));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePersonal(string id)
    {
        var (user, error) = RoleGuard.RequireAuth(HttpContext);
        if (error is not null) return error;

        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id);
        if (note is null) return NotFound();
        if (note.OwnerId != user!.Id || note.NoteType != NoteTypes.Personal) return RoleGuard.Forbidden();

        _db.Notes.Remove(note);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed()
    {
        var (user, error) = RoleGuard.RequireAuth(HttpContext);
        if (error is not null) return error;

        var notes = new List<Note>();

        var globals = await _db.Notes
            .Where(n => n.NoteType == NoteTypes.Global)
            .ToListAsync();
        notes.AddRange(globals);

        if (user!.Role == UserRoles.Student)
        {
            var myGroupIds = await _db.GroupMembers
                .Where(m => m.StudentId == user.Id && m.Status == GroupMemberStatus.Active)
                .Select(m => m.GroupId)
                .ToListAsync();
            var groupNotes = await _db.Notes
                .Where(n => n.NoteType == NoteTypes.Group && n.TargetGroupId != null && myGroupIds.Contains(n.TargetGroupId!))
                .ToListAsync();
            notes.AddRange(groupNotes);
        }
        else if (user.Role == UserRoles.Teacher)
        {
            var myGroupNotes = await _db.Notes
                .Where(n => n.NoteType == NoteTypes.Group && n.OwnerId == user.Id)
                .ToListAsync();
            notes.AddRange(myGroupNotes);
        }

        return Ok(notes.OrderByDescending(n => n.CreatedDate).Select(ToDto));
    }

    [HttpPost("global")]
    public async Task<IActionResult> CreateGlobal([FromBody] CreateNoteRequest req)
    {
        var (user, error) = RoleGuard.RequireRole(HttpContext, UserRoles.Head);
        if (error is not null) return error;

        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest(new { error = "Заголовок не может быть пустым." });

        var now = DateTime.UtcNow;
        var note = new Note
        {
            Title = req.Title,
            Content = req.Content ?? string.Empty,
            OwnerId = user!.Id,
            NoteType = NoteTypes.Global,
            CreatedDate = now,
            ModifiedDate = now
        };
        _db.Notes.Add(note);
        await _db.SaveChangesAsync();
        return Ok(ToDto(note));
    }

    internal static NoteDto ToDto(Note n) =>
        new(n.Id, n.Title, n.Content, n.OwnerId, n.NoteType, n.TargetGroupId, n.CreatedDate, n.ModifiedDate);
}
