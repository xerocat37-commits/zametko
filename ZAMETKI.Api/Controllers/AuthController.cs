using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZAMETKI.Api.Contracts;
using ZAMETKI.Api.Data;
using ZAMETKI.Api.Domain;
using ZAMETKI.Api.Middleware;
using ZAMETKI.Api.Services;

namespace ZAMETKI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher _hasher;
    private readonly TokenService _tokens;

    public AuthController(AppDbContext db, PasswordHasher hasher, TokenService tokens)
    {
        _db = db;
        _hasher = hasher;
        _tokens = tokens;
    }

    [HttpPost("register/student")]
    public async Task<IActionResult> RegisterStudent([FromBody] RegisterStudentRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.GroupName) || string.IsNullOrWhiteSpace(req.FullName) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "Заполните группу, ФИО и пароль." });

        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Name == req.GroupName);
        if (group is null)
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Вас не пригласили в эту группу." });

        var invite = await _db.GroupMembers.FirstOrDefaultAsync(m =>
            m.GroupId == group.Id &&
            m.StudentFullName == req.FullName &&
            m.Status == GroupMemberStatus.Invited);
        if (invite is null)
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Вас не пригласили в эту группу." });

        if (await _db.Users.AnyAsync(u => u.FullName == req.FullName))
            return Conflict(new { error = "Пользователь с таким ФИО уже существует." });

        var user = new User
        {
            FullName = req.FullName,
            GroupName = req.GroupName,
            Role = UserRoles.Student,
            PasswordHash = _hasher.Hash(req.Password),
            CreatedDate = DateTime.UtcNow
        };
        _db.Users.Add(user);

        invite.StudentId = user.Id;
        invite.Status = GroupMemberStatus.Active;

        await _db.SaveChangesAsync();

        var session = await _tokens.CreateSessionAsync(user.Id);
        return Ok(new AuthResponse(session.Id, ToDto(user)));
    }

    [HttpPost("register/teacher")]
    public async Task<IActionResult> RegisterTeacher([FromBody] RegisterTeacherRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.FullName) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "Заполните ФИО и пароль." });

        if (await _db.Users.AnyAsync(u => u.FullName == req.FullName))
            return Conflict(new { error = "Пользователь с таким ФИО уже существует." });

        var user = new User
        {
            FullName = req.FullName,
            Role = UserRoles.Teacher,
            PasswordHash = _hasher.Hash(req.Password),
            CreatedDate = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var session = await _tokens.CreateSessionAsync(user.Id);
        return Ok(new AuthResponse(session.Id, ToDto(user)));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.FullName) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "Заполните ФИО и пароль." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.FullName == req.FullName);
        if (user is null || !_hasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { error = "Неверные учётные данные." });

        var session = await _tokens.CreateSessionAsync(user.Id);
        return Ok(new AuthResponse(session.Id, ToDto(user)));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var user = HttpContext.GetUser();
        if (user is null) return Unauthorized();

        var token = HttpContext.GetToken();
        if (token is not null) await _tokens.DeleteSessionAsync(token);
        return NoContent();
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var user = HttpContext.GetUser();
        if (user is null) return Unauthorized();
        return Ok(ToDto(user));
    }

    private static UserDto ToDto(User u) => new(u.Id, u.FullName, u.GroupName, u.Role);
}
