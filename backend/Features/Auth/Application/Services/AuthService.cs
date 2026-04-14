using System.Security.Claims;
using backend.Core;
using backend.Features.Auth.Infrastructure.Authentication;
using backend.Extensions.Auth.Abstractions;
using backend.Extensions.Auth.Models;
using backend.Features.Auth.Application.DTOs;
using backend.Features.Auth.Domain;
using backend.Features.Auth.Infrastructure.Repositories;
using backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharedTokenPayload = backend.Extensions.Auth.Models.TokenPayload;

namespace backend.Features.Auth.Application.Services;

public sealed class AuthService(
    IAuthRepository repository,
    IAuthExtension authExtension,
    IUnitOfWork unitOfWork,
    ILogger<AuthService> logger
) : IAuthService
{
    private readonly IAuthRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IAuthExtension _authExtension = authExtension ?? throw new ArgumentNullException(nameof(authExtension));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ILogger<AuthService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<TokenResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request is null)
        {
            throw LogicException.NullValue(nameof(request));
        }

        _logger.LogInformation("Attempting authentication for {Email}", request.Email);

        User? user = await _repository.GetUserByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found for {Email}", request.Email);
            return null;
        }

        bool passwordMatches = await _authExtension.VerifyPasswordAsync(request.Password, user.HashedPassword)
            .ConfigureAwait(false);

        if (!passwordMatches)
        {
            _logger.LogWarning("Login failed: invalid password for {Email}", request.Email);
            return null;
        }

        string role = user.Role?.Name ?? "User";
        string accessToken = _authExtension.CreateAccessToken(new SharedTokenPayload(user.Email, role));

        return new TokenResponse(accessToken, "bearer");
    }

    public async Task<UserResponse> RegisterAsync(
        UserCreateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request is null)
        {
            throw LogicException.NullValue(nameof(request));
        }

        _logger.LogInformation("Registering user {Email}", request.Email);

        User? existingUser = await _repository.GetUserByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);

        if (existingUser is not null)
        {
            throw new LogicException($"Email already registered: {request.Email}");
        }

        Role? role = await _repository.GetRoleByNameAsync(request.Role, cancellationToken)
            .ConfigureAwait(false);

        if (role is null && !string.Equals(request.Role, "User", StringComparison.OrdinalIgnoreCase))
        {
            role = await _repository.GetRoleByNameAsync("User", cancellationToken)
                .ConfigureAwait(false);
        }

        if (role is null)
        {
            throw new LogicException($"Role '{request.Role}' was not found and fallback role 'User' is unavailable.");
        }

        string hashedPassword = await _authExtension.GetPasswordHashAsync(request.Password)
            .ConfigureAwait(false);

        User user = new()
        {
            Email = request.Email,
            HashedPassword = hashedPassword,
            RoleId = role.Id,
            FullName = request.FullName,
            Role = role,
        };

        await _repository.AddUserAsync(user, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new UserResponse(user.Email, user.FullName, new List<string> { role.Name });
    }

    public async Task<string> SignInWithGoogleAsync(ClaimsPrincipal principal, HttpContext httpContext)
    {
        if (principal is null)
        {
            throw LogicException.NullValue(nameof(principal));
        }

        if (httpContext is null)
        {
            throw LogicException.NullValue(nameof(httpContext));
        }

        string googleId = GetRequiredClaim(principal, ClaimTypes.NameIdentifier, "sub", "id");
        string email = GetRequiredClaim(principal, ClaimTypes.Email, "email");
        string? fullName = GetOptionalClaim(principal, ClaimTypes.Name, "name");
        string? avatarUrl = GetOptionalClaim(principal, "picture");

        _logger.LogInformation("Signing in Google user {Email}", email);

        User? user = await _repository
            .FindByGoogleIdAsync(googleId, httpContext.RequestAborted)
            .ConfigureAwait(false);

        if (user is null)
        {
            Role? role = await _repository
                .GetRoleByNameAsync("User", httpContext.RequestAborted)
                .ConfigureAwait(false);

            if (role is null)
            {
                throw new LogicException("Role 'User' was not found.");
            }

            string hashedPassword = await _authExtension
                .GetPasswordHashAsync(Guid.NewGuid().ToString("N"))
                .ConfigureAwait(false);

            user = new User
            {
                Email = email,
                FullName = fullName,
                HashedPassword = hashedPassword,
                RoleId = role.Id,
                Role = role,
                GoogleId = googleId,
                AvatarUrl = avatarUrl,
                EmailConfirmed = true,
            };

            await _repository.AddUserAsync(user, httpContext.RequestAborted).ConfigureAwait(false);
            await _unitOfWork.CommitAsync(httpContext.RequestAborted).ConfigureAwait(false);
        }

        string roleName = user.Role?.Name ?? "User";
        string accessToken = _authExtension.CreateAccessToken(
            new SharedTokenPayload(user.Email, roleName)
        );

        httpContext.Response.Cookies.Append(
            GoogleAuthDefaults.AccessTokenCookieName,
            accessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
            }
        );

        return accessToken;
    }

    public async Task LogoutAsync(string tokenJti, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenJti))
        {
            throw LogicException.NullValue(nameof(tokenJti));
        }

        _logger.LogInformation("Revoking token jti {Jti}", tokenJti);
        await _authExtension.AddToBlacklistAsync(tokenJti).ConfigureAwait(false);
    }

    public async Task<UserResponse?> GetMeAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw LogicException.NullValue(nameof(email));
        }

        User? user = await _repository.GetUserByEmailAsync(email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        string role = user.Role?.Name ?? "User";
        return new UserResponse(user.Email, user.FullName, new List<string> { role });
    }

    private static string GetRequiredClaim(ClaimsPrincipal principal, params string[] claimTypes)
    {
        string? value = GetOptionalClaim(principal, claimTypes);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw LogicException.NullValue(claimTypes[0]);
        }

        return value;
    }

    private static string? GetOptionalClaim(ClaimsPrincipal principal, params string[] claimTypes)
    {
        foreach (string claimType in claimTypes)
        {
            string? value = principal.FindFirstValue(claimType);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}