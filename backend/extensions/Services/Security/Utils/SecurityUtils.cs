using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace backend.Extensions.Security.Utils;

internal static class SecurityUtils
{
    public static string ResolveIdentifier(HttpContext context)
    {
        string forwarded = context.Request.Headers["X-Forwarded-For"].ToString();

        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            string? claimUserId =
                context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub")
                ?? context.User.FindFirstValue(ClaimTypes.Name);

            return !string.IsNullOrWhiteSpace(claimUserId) ? claimUserId : "auth-user";
        }

        if (context.Items.TryGetValue("user_id", out object? userId) && userId is not null)
        {
            string? resolvedUserId = userId.ToString();

            if (!string.IsNullOrWhiteSpace(resolvedUserId))
            {
                return resolvedUserId;
            }
        }

        if (!context.Items.TryGetValue("user", out object? user) || user is null)
            return context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        string? resolvedUser = user.ToString();

        if (!string.IsNullOrWhiteSpace(resolvedUser))
        {
            return resolvedUser;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }
}