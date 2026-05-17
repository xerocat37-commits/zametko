namespace ZAMETKI.Api.Contracts;

public record RegisterStudentRequest(string GroupName, string FullName, string Password);

public record RegisterTeacherRequest(string FullName, string Password);

public record LoginRequest(string FullName, string Password);

public record UserDto(string Id, string FullName, string? GroupName, string Role);

public record AuthResponse(string Token, UserDto User);
