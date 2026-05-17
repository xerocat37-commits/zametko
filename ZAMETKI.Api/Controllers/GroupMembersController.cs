using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZAMETKI.Api.Authorization;
using ZAMETKI.Api.Contracts;
using ZAMETKI.Api.Data;
using ZAMETKI.Api.Domain;

namespace ZAMETKI.Api.Controllers;

[ApiController]
[Route("api/groups/{groupId}/members")]
public class GroupMembersController : ControllerBase
{
    private readonly AppDbContext _db;

    public GroupMembersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(string groupId)
    {
        var (user, error) = RoleGuard.RequireRole(HttpContext, UserRoles.Teacher);
        if (error is not null) return error;

        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group is null) return NotFound();
        if (group.TeacherId != user!.Id) return RoleGuard.Forbidden();

        var members = await _db.GroupMembers
            .Where(m => m.GroupId == groupId)
            .OrderBy(m => m.StudentFullName)
            .ToListAsync();
        return Ok(members.Select(ToDto));
    }

    [HttpPost]
    public async Task<IActionResult> Invite(string groupId, [FromBody] InviteStudentRequest req)
    {
        var (user, error) = RoleGuard.RequireRole(HttpContext, UserRoles.Teacher);
        if (error is not null) return error;

        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group is null) return NotFound();
        if (group.TeacherId != user!.Id) return RoleGuard.Forbidden();

        if (string.IsNullOrWhiteSpace(req.StudentFullName))
            return BadRequest(new { error = "ФИО студента не может быть пустым." });

        if (await _db.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.StudentFullName == req.StudentFullName))
            return Conflict(new { error = "Студент уже приглашён в эту группу." });

        var member = new GroupMember
        {
            GroupId = groupId,
            StudentFullName = req.StudentFullName,
            Status = GroupMemberStatus.Invited
        };
        _db.GroupMembers.Add(member);
        await _db.SaveChangesAsync();
        return Ok(ToDto(member));
    }

    private static GroupMemberDto ToDto(GroupMember m) =>
        new(m.Id, m.GroupId, m.StudentFullName, m.StudentId, m.Status);
}
