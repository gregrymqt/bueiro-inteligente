using System.Security.Claims;
using backend.Core;
using backend.Extensions.Auth.Abstractions;
using backend.Extensions.Auth.Models;
using backend.Features.Auth.Application.DTOs;
using backend.Features.Auth.Domain;
using backend.Features.Auth.Infrastructure.Authentication;
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
    AppSettings settings,
    ILogger<AuthService> logger
) : IAuthService
{
    private readonly IAuthRepository _repository =
        repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IAuthExtension _authExtension =
        authExtension ?? throw new ArgumentNullException(nameof(authExtension));
    private readonly IUnitOfWork _unitOfWork =
        unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly AppSettings _settings =
        settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly ILogger<AuthService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private static readonly string[] PrivilegedRoleNames = new[] { "Admin", "Manager", "User" };
    private const string DefaultRoleName = "User";

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

        User? user = await _repository
            .GetUserByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found for {Email}", request.Email);
            return null;
        }

        bool passwordMatches = await _authExtension
            .VerifyPasswordAsync(request.Password, user.HashedPassword)
            .ConfigureAwait(false);

        if (!passwordMatches)
        {
            _logger.LogWarning("Login failed: invalid password for {Email}", request.Email);
            return null;
        }

        IReadOnlyList<string> roleNames = ResolveUserRoleNames(user.Email, user.Roles);
        string accessToken = _authExtension.CreateAccessToken(
            BuildTokenPayload(user.Email, roleNames)
        );

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

        User? existingUser = await _repository
            .GetUserByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);

        if (existingUser is not null)
        {
            throw new LogicException($"Email already registered: {request.Email}");
        }

        IReadOnlyList<string> assignedRoleNames = await ResolveAssignedRoleNamesAsync(
                request.Email,
                request.Role,
                cancellationToken
            )
            .ConfigureAwait(false);

        IReadOnlyList<Role> roles = await GetRolesByNamesAsync(assignedRoleNames, cancellationToken)
            .ConfigureAwait(false);

        string hashedPassword = await _authExtension
            .GetPasswordHashAsync(request.Password)
            .ConfigureAwait(false);

        User user = new()
        {
            Email = request.Email,
            HashedPassword = hashedPassword,
            FullName = request.FullName,
            Roles = roles.ToList(),
        };

        await _repository.AddUserAsync(user, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new UserResponse(user.Email, user.FullName, assignedRoleNames);
    }

    public async Task<string> SignInWithGoogleAsync(
        ClaimsPrincipal principal,
        HttpContext httpContext
    )
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
            IReadOnlyList<string> assignedRoleNames = await ResolveAssignedRoleNamesAsync(
                    email,
                    DefaultRoleName,
                    httpContext.RequestAborted
                )
                .ConfigureAwait(false);

            IReadOnlyList<Role> roles = await GetRolesByNamesAsync(
                    assignedRoleNames,
                    httpContext.RequestAborted
                )
                .ConfigureAwait(false);

            string hashedPassword = await _authExtension
                .GetPasswordHashAsync(Guid.NewGuid().ToString("N"))
                .ConfigureAwait(false);

            user = new User
            {
                Email = email,
                FullName = fullName,
                HashedPassword = hashedPassword,
                GoogleId = googleId,
                AvatarUrl = avatarUrl,
                EmailConfirmed = true,
                Roles = roles.ToList(),
            };

            await _repository.AddUserAsync(user, httpContext.RequestAborted).ConfigureAwait(false);
            await _unitOfWork.CommitAsync(httpContext.RequestAborted).ConfigureAwait(false);
        }

        IReadOnlyList<string> roleNames = ResolveUserRoleNames(email, user.Roles);
        string accessToken = _authExtension.CreateAccessToken(
            BuildTokenPayload(user.Email, roleNames)
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

    private async Task<IReadOnlyList<string>> ResolveAssignedRoleNamesAsync(
        string email,
        string requestedRole,
        CancellationToken cancellationToken
    )
    {
        if (IsPrivilegedUser(email))
        {
            return PrivilegedRoleNames;
        }

        Role? role = await _repository
            .GetRoleByNameAsync(requestedRole, cancellationToken)
            .ConfigureAwait(false);

        if (
            role is null
            && !string.Equals(requestedRole, DefaultRoleName, StringComparison.OrdinalIgnoreCase)
        )
        {
            role = await _repository
                .GetRoleByNameAsync(DefaultRoleName, cancellationToken)
                .ConfigureAwait(false);
        }

        if (role is null)
        {
            throw new LogicException(
                $"Role '{requestedRole}' was not found and fallback role 'User' is unavailable."
            );
        }

        return new[] { role.Name };
    }

    private async Task<IReadOnlyList<Role>> GetRolesByNamesAsync(
        IReadOnlyList<string> roleNames,
        CancellationToken cancellationToken
    )
    {
        List<Role> roles = new(roleNames.Count);

        foreach (string roleName in NormalizeRoleNames(roleNames))
        {
            Role? role = await _repository
                .GetRoleByNameAsync(roleName, cancellationToken)
                .ConfigureAwait(false);

            if (role is null)
            {
                throw new LogicException($"Role '{roleName}' was not found.");
            }

            roles.Add(role);
        }

        return roles;
    }

    private IReadOnlyList<string> ResolveUserRoleNames(string email, IEnumerable<Role> roles)
    {
        if (IsPrivilegedUser(email))
        {
            return PrivilegedRoleNames;
        }

        return NormalizeRoleNames(roles.Select(role => role.Name));
    }

    private bool IsPrivilegedUser(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || _settings.EmailUsersAdmin.Length == 0)
        {
            return false;
        }

        string normalizedEmail = email.Trim();

        return _settings.EmailUsersAdmin.Any(adminEmail =>
            !string.IsNullOrWhiteSpace(adminEmail)
            && string.Equals(adminEmail.Trim(), normalizedEmail, StringComparison.OrdinalIgnoreCase)
        );
    }

    private static IReadOnlyList<string> NormalizeRoleNames(IEnumerable<string> roleNames)
    {
        return roleNames
            .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
            .Select(roleName => roleName.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(GetRolePriority)
            .ToArray();
    }

    private static int GetRolePriority(string roleName)
    {
        return roleName.ToUpperInvariant() switch
        {
            "ADMIN" => 0,
            "MANAGER" => 1,
            "USER" => 2,
            _ => 3,
        };
    }

    private static SharedTokenPayload BuildTokenPayload(
        string email,
        IReadOnlyList<string> roleNames
    )
    {
        IReadOnlyList<string> normalizedRoleNames = NormalizeRoleNames(roleNames);
        string primaryRole = normalizedRoleNames.FirstOrDefault() ?? DefaultRoleName;

        Dictionary<string, object?> additionalClaims = new(StringComparer.Ordinal)
        {
            ["roles"] = normalizedRoleNames.ToArray(),
        };

        return new SharedTokenPayload(email, primaryRole, additionalClaims);
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

        User? user = await _repository
            .GetUserByEmailAsync(email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        IReadOnlyList<string> roleNames = ResolveUserRoleNames(email, user.Roles);
        return new UserResponse(user.Email, user.FullName, roleNames);
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
