using System.Security.Claims;
using System.Text.Encodings.Web;
using backend.Extensions.Auth.Abstractions;
using backend.Extensions.Auth;
using backend.Extensions.Auth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Features.Auth.Infrastructure.Authentication;

public static class BueiroInteligenteAuthenticationDefaults
{
    public const string Scheme = "BueiroInteligenteBearer";
}

// C# 12: Primary Constructor eliminando o boilerplate de campos privados
public sealed class BueiroInteligenteAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IAuthExtension authExtension
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly IAuthExtension _authExtension = authExtension ?? throw new ArgumentNullException(nameof(authExtension));

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Simplificação da busca do token usando expressões de atribuição
        string? token = AuthExtension.NormalizeBearerToken(Request.Headers.Authorization.ToString());

        if (string.IsNullOrWhiteSpace(token) && Request.Cookies.TryGetValue(GoogleAuthDefaults.AccessTokenCookieName, out var cookieToken))
        {
            token = AuthExtension.NormalizeBearerToken(cookieToken);
        }

        if (string.IsNullOrWhiteSpace(token) && Request.Query.TryGetValue("access_token", out var queryToken))
        {
            token = AuthExtension.NormalizeBearerToken(queryToken.ToString());
        }

        if (string.IsNullOrWhiteSpace(token)) return AuthenticateResult.NoResult();

        try
        {
            var currentUser = await _authExtension.GetCurrentUserAsync(token, Context.RequestAborted).ConfigureAwait(false);

            // C# 12: Collection Expressions para as claims iniciais
            List<Claim> claims = [
                new(ClaimTypes.NameIdentifier, currentUser.Email),
                new(ClaimTypes.Email, currentUser.Email),
                new(ClaimTypes.Name, currentUser.Email),
                new("jti", currentUser.Jti)
            ];

            // Lógica de roles enxuta
            var roles = currentUser.Roles.Count > 0 ? currentUser.Roles : [currentUser.Role];

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(
                claims,
                BueiroInteligenteAuthenticationDefaults.Scheme,
                ClaimTypes.Name,
                ClaimTypes.Role
            );
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, BueiroInteligenteAuthenticationDefaults.Scheme);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            return AuthenticateResult.Fail(ex);
        }
    }
}