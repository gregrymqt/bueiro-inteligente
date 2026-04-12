using System.Security.Claims;
using backend.Core;
using backend.Features.Auth.Application.DTOs;
using backend.Features.Auth.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Auth.Presentation.Controllers;

[ApiController]
[Route("auth")]
[Authorize(Roles = "Admin,Manager,User")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService ?? throw new ArgumentNullException(nameof(authService));

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            TokenResponse? token = await _authService.LoginAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (token is null)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "Incorrect email or password.",
                    Status = StatusCodes.Status401Unauthorized,
                });
            }

            return Ok(token);
        }
        catch (ConnectionException exception)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails
                {
                    Title = "Connection error",
                    Detail = exception.Message,
                    Status = StatusCodes.Status503ServiceUnavailable,
                }
            );
        }
        catch (LogicException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation error",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest,
            });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponse>> Register(
        [FromBody] UserCreateRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            UserResponse result = await _authService.RegisterAsync(request, cancellationToken)
                .ConfigureAwait(false);

            return Created("/auth/me", result);
        }
        catch (ConnectionException exception)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails
                {
                    Title = "Connection error",
                    Detail = exception.Message,
                    Status = StatusCodes.Status503ServiceUnavailable,
                }
            );
        }
        catch (LogicException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation error",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest,
            });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        try
        {
            string? tokenJti = User.FindFirstValue("jti");

            if (string.IsNullOrWhiteSpace(tokenJti))
            {
                throw LogicException.NullValue("jti");
            }

            await _authService.LogoutAsync(tokenJti, cancellationToken).ConfigureAwait(false);

            return Ok(new { message = "Logout successful" });
        }
        catch (ConnectionException exception)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails
                {
                    Title = "Connection error",
                    Detail = exception.Message,
                    Status = StatusCodes.Status503ServiceUnavailable,
                }
            );
        }
        catch (LogicException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation error",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest,
            });
        }
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetMe(CancellationToken cancellationToken)
    {
        try
        {
            string? email = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(email))
            {
                throw LogicException.NullValue("email");
            }

            UserResponse? result = await _authService.GetMeAsync(email, cancellationToken)
                .ConfigureAwait(false);

            if (result is null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not found",
                    Detail = "User not found.",
                    Status = StatusCodes.Status404NotFound,
                });
            }

            return Ok(result);
        }
        catch (ConnectionException exception)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails
                {
                    Title = "Connection error",
                    Detail = exception.Message,
                    Status = StatusCodes.Status503ServiceUnavailable,
                }
            );
        }
        catch (LogicException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation error",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest,
            });
        }
    }
}