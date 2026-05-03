using System.Security.Claims;
using backend.Core;
using backend.Core.Settings;
using backend.Extensions.Auth;
using backend.extensions.Services.Auth.Logic;
using backend.Features.Auth.Application.DTOs;
using backend.Features.Auth.Application.Interfaces;
using backend.Features.Auth.Application.Services;
using backend.Features.Auth.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace backend.Features.Auth.Presentation.Controllers;

public sealed class AuthController(
    IAuthService authService,
    IOptions<GoogleSettings> googleSettings
) : ApiControllerBase
{
    // C# 12: Campos capturados diretamente do construtor primário
    private readonly IAuthService _authService =
        authService ?? throw new ArgumentNullException(nameof(authService));
    private readonly GoogleSettings _googleSettings =
        googleSettings?.Value ?? throw new ArgumentNullException(nameof(googleSettings));

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct
    )
    {
        var token = await _authService.LoginAsync(request, ct).ConfigureAwait(false);
        return token is null ? Unauthorized() : Ok(token);
    }

    [HttpGet("google-login")]
    [AllowAnonymous]
    public IActionResult GoogleLogin(
        [FromQuery(Name = "frontend_redirect")] string? frontendRedirectUrl = null
    )
    {
        if (
            string.IsNullOrWhiteSpace(_googleSettings.GoogleClientId)
            || string.IsNullOrWhiteSpace(_googleSettings.GoogleClientSecret)
        )
        {
            throw new ConnectionException(
                "Google authentication",
                "Google authentication is not configured on this environment."
            );
        }

        var resolvedUrl = GoogleRedirectUrlResolver.ResolveFrontendRedirectUrl(
            frontendRedirectUrl,
            _googleSettings.AllowedOrigins,
            _googleSettings.GoogleFrontendRedirectUrl
        );

        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri =
                    $"{GoogleAuthDefaults.RedirectPath}?frontend_redirect={Uri.EscapeDataString(resolvedUrl)}",
            },
            GoogleDefaults.AuthenticationScheme
        );
    }

    [HttpGet("google-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback(
        [FromQuery(Name = "frontend_redirect")] string? frontendRedirectUrl,
        CancellationToken ct
    )
    {
        if (
            string.IsNullOrWhiteSpace(_googleSettings.GoogleClientId)
            || string.IsNullOrWhiteSpace(_googleSettings.GoogleClientSecret)
        )
        {
            throw new ConnectionException(
                "Google authentication",
                "Google authentication is not configured on this environment."
            );
        }

        var authResult = await HttpContext
            .AuthenticateAsync(IdentityConstants.ExternalScheme)
            .ConfigureAwait(false);

        if (!authResult.Succeeded || authResult.Principal is null)
            return Unauthorized();

        var accessToken = await _authService
            .SignInWithGoogleAsync(authResult.Principal, HttpContext)
            .ConfigureAwait(false);
        var resolvedUrl = GoogleRedirectUrlResolver.ResolveFrontendRedirectUrl(
            frontendRedirectUrl,
            _googleSettings.AllowedOrigins,
            _googleSettings.GoogleFrontendRedirectUrl
        );

        return Redirect($"{resolvedUrl}#token={Uri.EscapeDataString(accessToken)}");
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponse>> Register(
    [FromBody] UserCreateRequest request,
    CancellationToken ct
)
    {
        try
        {
            var result = await _authService.RegisterAsync(request, ct).ConfigureAwait(false);
            return Created("/auth/users/me", result);
        }
        catch (Exception ex)
        {
            // TÁTICA DE GUERRILHA: Ignora o Serilog e escreve direto na veia do console do Docker
            Console.WriteLine("\n================= FANTASMA REVELADO =================");
            Console.WriteLine($"MENSAGEM: {ex.Message}");
            Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"CAUSA RAIZ: {ex.InnerException.Message}");
            }
            Console.WriteLine("=====================================================\n");

            throw; // Repassa o erro para o seu handler cuspir o JSON 500 no front
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var jti = User.FindFirstValue("jti") ?? throw LogicException.NullValue("jti");
        await _authService.LogoutAsync(jti, ct).ConfigureAwait(false);
        return Ok(new { message = "Logout successful" });
    }

    [HttpGet("users/me")]
    public async Task<ActionResult<UserResponse>> GetMe(CancellationToken ct)
    {
        var email =
            User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw LogicException.NullValue("email");

        var result = await _authService.GetMeAsync(email, ct).ConfigureAwait(false);
        return result is null ? NotFound() : Ok(result);
    }
}
