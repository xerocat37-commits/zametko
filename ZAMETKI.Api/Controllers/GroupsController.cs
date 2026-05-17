using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZAMETKI.Api.Authorization;
using ZAMETKI.Api.Contracts;
using ZAMETKI.Api.Data;
using ZAMETKI.Api.Domain;

namespace ZAMETKI.Api.Controllers;

[ApiController]
[Route("api/groups")]
public class GroupsController : ControllerBase
{
    private readonly AppDbContext _db;

    public GroupsController(AppDbContext db) => _db = db;

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var (user, error) = RoleGuard.RequireRole(HttpContext, UserRoles.Teacher);
        if (error is not null) return error;

        var groups = await _db.Groups
            .Where(g => g.TeacherId == user!.Id)
            .OrderByDescending(g => g.CreatedDate)
            .ToListAsync();
        return Ok(groups.Select(ToDto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest req)
    {
        var (user, error) = RoleGuard.RequireRole(HttpContext, UserRoles.Teacher);
        if (error is not null) return error;

        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { error = "Название группы не может быть пустым." });

        if (await _db.Groups.AnyAsync(g => g.Name == req.Name))
            return Conflict(new { error = "Группа с таким названием уже существует." });

        var group = new Group
        {
            Name = req.Name,
            TeacherId = user!.Id,
            CreatedDate = DateTime.UtcNow
        };
        _db.Groups.Add(group);
        await _db.SaveChangesAsync();
        return Ok(ToDto(group));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var (user, error) = RoleGuard.RequireAuth(HttpContext);
        if (error is not null) return error;

        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == id);
        if (group is null) return NotFound();

        if (!await IsTeacherOrActiveMember(group, user!)) return RoleGuard.Forbidden();
        return Ok(ToDto(group));
    }

    [HttpGet("{id}/notes")]
    public async Task<IActionResult> GetGroupNotes(string id)
    {
        var (user, error) = RoleGuard.RequireAuth(HttpContext);
        if (error is not null) return error;

        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == id);
        if (group is null) return NotFound();

        if (!await IsTeacherOrActiveMember(group, user!)) return RoleGuard.Forbidden();

        var notes = await _db.Notes
            .Where(n => n.NoteType == NoteTypes.Group && n.TargetGroupId == group.Id)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
        return Ok(notes.Select(NotesController.ToDto));
    }

    [HttpPost("{id}/notes")]
    public async Task<IActionResult> CreateGroupNote(string id, [FromBody] CreateNoteRequest req)
    {
        var (user, error) = RoleGuard.RequireRole(HttpContext, UserRoles.Teacher);
        if (error is not null) return error;

        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == id);
        if (group is null) return NotFound();
        if (group.TeacherId != user!.Id) return RoleGuard.Forbidden();

        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest(new { error = "Заголовок не может быть пустым." });

        var now = DateTime.UtcNow;
        var note = new Note
        {
            Title = req.Title,
            Content = req.Content ?? string.Empty,
            OwnerId = user.Id,
            NoteType = NoteTypes.Group,
            TargetGroupId = group.Id,
            CreatedDate = now,
            ModifiedDate = now
        };
        _db.Notes.Add(note);
        await _db.SaveChangesAsync();
        return Ok(NotesController.ToDto(note));
    }

    private async Task<bool> IsTeacherOrActiveMember(Group group, User user)
    {
        if (group.TeacherId == user.Id) return true;
        return await _db.GroupMembers.AnyAsync(m =>
            m.GroupId == group.Id &&
            m.StudentId == user.Id &&
            m.Status == GroupMemberStatus.Active);
    }

    internal static GroupDto ToDto(Group g) => new(g.Id, g.Name, g.TeacherId, g.CreatedDate);
}
