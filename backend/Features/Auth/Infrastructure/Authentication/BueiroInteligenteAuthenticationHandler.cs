using System.Security.Claims;
using System.Text.Encodings.Web;
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

public sealed class BueiroInteligenteAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AuthExtension _authExtension;

    [Obsolete]
    public BueiroInteligenteAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        AuthExtension authExtension
    )
        : base(options, logger, encoder, clock)
    {
        _authExtension = authExtension ?? throw new ArgumentNullException(nameof(authExtension));
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? token = AuthExtension.NormalizeBearerToken(
            Request.Headers.Authorization.ToString()
        );

        if (string.IsNullOrWhiteSpace(token))
        {
            return AuthenticateResult.NoResult();
        }

        try
        {
            UserTokenData currentUser = await _authExtension
                .GetCurrentUserAsync(token, Context.RequestAborted)
                .ConfigureAwait(false);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, currentUser.Email),
                new(ClaimTypes.Email, currentUser.Email),
                new(ClaimTypes.Name, currentUser.Email),
                new(ClaimTypes.Role, currentUser.Role),
                new("jti", currentUser.Jti),
            };

            var identity = new ClaimsIdentity(
                claims,
                Scheme.Name,
                ClaimTypes.Name,
                ClaimTypes.Role
            );

            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception exception)
        {
            return AuthenticateResult.Fail(exception);
        }
    }
}
