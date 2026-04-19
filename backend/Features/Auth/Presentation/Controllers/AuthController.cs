using System.Security.Claims;
using backend.Core;
using backend.Core.Settings;
using backend.Extensions.Auth;
using backend.Features.Auth.Application.DTOs;
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
    ) =>
        await ExecuteAsync(async () =>
        {
            var token = await _authService.LoginAsync(request, ct).ConfigureAwait(false);
            return token is null
                ? Unauthorized(CreateProblem("Unauthorized", "Incorrect email or password.", 401))
                : Ok(token);
        });

    [HttpGet("google-login")]
    [AllowAnonymous]
    public IActionResult GoogleLogin(
        [FromQuery(Name = "frontend_redirect")] string? frontendRedirectUrl = null
    )
    {
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
    ) =>
        await ExecuteAsync(async () =>
        {
            var authResult = await HttpContext
                .AuthenticateAsync(IdentityConstants.ExternalScheme)
                .ConfigureAwait(false);

            if (!authResult.Succeeded || authResult.Principal is null)
                return Unauthorized(
                    CreateProblem("Unauthorized", "Google authentication failed.", 401)
                );

            var accessToken = await _authService
                .SignInWithGoogleAsync(authResult.Principal, HttpContext)
                .ConfigureAwait(false);
            var resolvedUrl = GoogleRedirectUrlResolver.ResolveFrontendRedirectUrl(
                frontendRedirectUrl,
                _googleSettings.AllowedOrigins,
                _googleSettings.GoogleFrontendRedirectUrl
            );

            return Redirect($"{resolvedUrl}#token={Uri.EscapeDataString(accessToken)}");
        });

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponse>> Register(
        [FromBody] UserCreateRequest request,
        CancellationToken ct
    ) =>
        await ExecuteAsync(async () =>
        {
            var result = await _authService.RegisterAsync(request, ct).ConfigureAwait(false);
            return Created("/auth/users/me", result);
        });

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct) =>
        await ExecuteAsync(async () =>
        {
            var jti = User.FindFirstValue("jti") ?? throw LogicException.NullValue("jti");
            await _authService.LogoutAsync(jti, ct).ConfigureAwait(false);
            return Ok(new { message = "Logout successful" });
        });

    [HttpGet("users/me")]
    public async Task<ActionResult<UserResponse>> GetMe(CancellationToken ct) =>
        await ExecuteAsync(async () =>
        {
            var email =
                User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw LogicException.NullValue("email");

            var result = await _authService.GetMeAsync(email, ct).ConfigureAwait(false);
            return result is null
                ? NotFound(CreateProblem("Not found", "User not found.", 404))
                : Ok(result);
        });

    #region Helpers Enxutos

    // Centraliza o tratamento de exceções para remover o excesso de try-catch
    private async Task<ActionResult> ExecuteAsync(Func<Task<ActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (ConnectionException ex)
        {
            return StatusCode(503, CreateProblem("Connection error", ex.Message, 503));
        }
        catch (LogicException ex)
        {
            return BadRequest(CreateProblem("Validation error", ex.Message, 400));
        }
    }

    private static ProblemDetails CreateProblem(string title, string detail, int status) =>
        new()
        {
            Title = title,
            Detail = detail,
            Status = status,
        };

    #endregion
}
