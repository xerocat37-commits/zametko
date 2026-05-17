using ZAMETKI.Api.Domain;
using ZAMETKI.Api.Services;

namespace ZAMETKI.Api.Middleware;

public class BearerAuthMiddleware
{
    public const string UserItemKey = "User";

    private readonly RequestDelegate _next;

    public BearerAuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx, TokenService tokens)
    {
        var header = ctx.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(header) && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = header.Substring("Bearer ".Length).Trim();
            if (!string.IsNullOrEmpty(token))
            {
                var user = await tokens.GetUserBySessionAsync(token);
                if (user is not null)
                {
                    ctx.Items[UserItemKey] = user;
                    ctx.Items["Token"] = token;
                }
            }
        }

        await _next(ctx);
    }
}

public static class HttpContextAuthExtensions
{
    public static User? GetUser(this HttpContext ctx) =>
        ctx.Items.TryGetValue(BearerAuthMiddleware.UserItemKey, out var v) ? v as User : null;

    public static string? GetToken(this HttpContext ctx) =>
        ctx.Items.TryGetValue("Token", out var v) ? v as string : null;
}
