using Microsoft.AspNetCore.Mvc;
using ZAMETKI.Api.Domain;
using ZAMETKI.Api.Middleware;

namespace ZAMETKI.Api.Authorization;

public static class RoleGuard
{
    public static (User? user, IActionResult? error) RequireAuth(HttpContext ctx)
    {
        var user = ctx.GetUser();
        if (user is null) return (null, new UnauthorizedResult());
        return (user, null);
    }

    public static (User? user, IActionResult? error) RequireRole(HttpContext ctx, params string[] roles)
    {
        var user = ctx.GetUser();
        if (user is null) return (null, new UnauthorizedResult());
        if (!roles.Contains(user.Role)) return (null, Forbidden());
        return (user, null);
    }

    public static IActionResult Forbidden() =>
        new ObjectResult(new { error = "Недостаточно прав." }) { StatusCode = StatusCodes.Status403Forbidden };
}
