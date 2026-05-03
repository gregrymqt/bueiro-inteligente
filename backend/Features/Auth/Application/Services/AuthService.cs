using System.Security.Claims;
using backend.Core;
using backend.Core.Settings;
using backend.Extensions.Auth.Models;
using backend.extensions.Services.Auth.Abstractions;
using backend.Features.Auth.Application.DTOs;
using backend.Features.Auth.Application.Interfaces;
using backend.Features.Auth.Domain;
using backend.Features.Auth.Domain.Entities;
using backend.Features.Auth.Domain.Interfaces;
using backend.Features.Auth.Infrastructure.Authentication;
using backend.Features.Auth.Infrastructure.Repositories;
using backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedTokenPayload = backend.Extensions.Auth.Models.TokenPayload;

namespace backend.Features.Auth.Application.Services;

public sealed class AuthService(
    IAuthRepository repository,
    IAuthExtension authExtension,
    IUnitOfWork unitOfWork,
    IOptions<GeneralSettings> settings,
    ILogger<AuthService> logger
) : IAuthService
{
    // C# 12: Mapeamento direto dos parâmetros do construtor primário para campos readonly
    private readonly IAuthRepository _repository =
        repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IAuthExtension _authExtension =
        authExtension ?? throw new ArgumentNullException(nameof(authExtension));
    private readonly IUnitOfWork _unitOfWork =
        unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly GeneralSettings _settings =
        settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    private readonly ILogger<AuthService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    // C# 12: Collection Expressions []
    private static readonly string[] PrivilegedRoleNames = ["Admin", "Manager", "User"];
    private const string DefaultRoleName = "User";

    public async Task<TokenResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation("Attempting authentication for {Email}", request?.Email);

        try
        {
            ArgumentNullException.ThrowIfNull(request);

            var user = await _repository
                .GetUserByEmailAsync(request.Email, ct)
                .ConfigureAwait(false);

            if (
                user is null
                || !await _authExtension
                    .VerifyPasswordAsync(request.Password, user.HashedPassword)
                    .ConfigureAwait(false)
            )
            {
                _logger.LogWarning("Login failed for {Email}", request.Email);
                return null;
            }

            var roleNames = ResolveUserRoleNames(user.Email, user.Roles);
            var accessToken = _authExtension.CreateAccessToken(
                BuildTokenPayload(user.Id, user.Email, roleNames)
            );

            return new TokenResponse(accessToken, "bearer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao autenticar usuário {Email}.", request?.Email);
            throw;
        }
    }

    public async Task<UserResponse> RegisterAsync(
        UserCreateRequest request,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation("Registering user {Email}", request?.Email);

        try
        {
            ArgumentNullException.ThrowIfNull(request);

            if (
                await _repository.GetUserByEmailAsync(request.Email, ct).ConfigureAwait(false)
                is not null
            )
                throw new LogicException($"Email already registered: {request.Email}");

            var assignedRoleNames = await ResolveAssignedRoleNamesAsync(
                    request.Email,
                    request.Role,
                    ct
                )
                .ConfigureAwait(false);
            var roles = await GetRolesByNamesAsync(assignedRoleNames, ct).ConfigureAwait(false);
            var hashedPassword = await _authExtension
                .GetPasswordHashAsync(request.Password)
                .ConfigureAwait(false);

            User user = new()
            {
                Email = request.Email,
                HashedPassword = hashedPassword,
                FullName = request.FullName,
                Roles = [.. roles], // C# 12: Spread operator
            };

            await _repository.AddUserAsync(user, ct).ConfigureAwait(false);
            await _unitOfWork.CommitAsync(ct).ConfigureAwait(false);

            return new UserResponse(user.Email, user.FullName, assignedRoleNames);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao registrar usuário {Email}. Nome: {FullName}. Papel: {Role}",
                request?.Email,
                request?.FullName,
                request?.Role
            );
            throw;
        }
    }

    public async Task<string> SignInWithGoogleAsync(
        ClaimsPrincipal principal,
        HttpContext httpContext
    )
    {
        string? googleId = null;
        string? email = null;

        try
        {
            ArgumentNullException.ThrowIfNull(principal);
            ArgumentNullException.ThrowIfNull(httpContext);

            googleId = GetRequiredClaim(principal, ClaimTypes.NameIdentifier, "sub", "id");
            email = GetRequiredClaim(principal, ClaimTypes.Email, "email");

            _logger.LogInformation("Signing in Google user {Email}", email);

            var user = await _repository
                .FindByGoogleIdAsync(googleId, httpContext.RequestAborted)
                .ConfigureAwait(false);

            if (user is null)
            {
                var assignedRoleNames = await ResolveAssignedRoleNamesAsync(
                        email,
                        DefaultRoleName,
                        httpContext.RequestAborted
                    )
                    .ConfigureAwait(false);
                var roles = await GetRolesByNamesAsync(
                        assignedRoleNames,
                        httpContext.RequestAborted
                    )
                    .ConfigureAwait(false);
                var hashedPassword = await _authExtension
                    .GetPasswordHashAsync(Guid.NewGuid().ToString("N"))
                    .ConfigureAwait(false);

                user = new User
                {
                    Email = email,
                    FullName = GetOptionalClaim(principal, ClaimTypes.Name, "name"),
                    HashedPassword = hashedPassword,
                    GoogleId = googleId,
                    AvatarUrl = GetOptionalClaim(principal, "picture"),
                    EmailConfirmed = true,
                    Roles = [.. roles],
                };

                await _repository
                    .AddUserAsync(user, httpContext.RequestAborted)
                    .ConfigureAwait(false);
                await _unitOfWork.CommitAsync(httpContext.RequestAborted).ConfigureAwait(false);
            }

            var accessToken = _authExtension.CreateAccessToken(
                BuildTokenPayload(user.Id, user.Email, ResolveUserRoleNames(email, user.Roles))
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
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao autenticar usuário via Google. Email: {Email}. GoogleId: {GoogleId}",
                email,
                googleId
            );
            throw;
        }
    }

    private async Task<IReadOnlyList<string>> ResolveAssignedRoleNamesAsync(
        string email,
        string requestedRole,
        CancellationToken ct
    )
    {
        if (IsPrivilegedUser(email))
            return PrivilegedRoleNames;

        var role =
            await _repository.GetRoleByNameAsync(requestedRole, ct).ConfigureAwait(false)
            ?? await _repository.GetRoleByNameAsync(DefaultRoleName, ct).ConfigureAwait(false);

        if (role is null)
            throw new LogicException(
                $"Role '{requestedRole}' not found and fallback 'User' unavailable."
            );

        return [role.Name];
    }

    private async Task<IReadOnlyList<Role>> GetRolesByNamesAsync(
        IReadOnlyList<string> roleNames,
        CancellationToken ct
    )
    {
        List<Role> roles = [];
        foreach (var name in NormalizeRoleNames(roleNames))
        {
            roles.Add(
                await _repository.GetRoleByNameAsync(name, ct).ConfigureAwait(false)
                    ?? throw new LogicException($"Role '{name}' not found.")
            );
        }
        return roles;
    }

    private IReadOnlyList<string> ResolveUserRoleNames(string email, IEnumerable<Role> roles) =>
        IsPrivilegedUser(email)
            ? PrivilegedRoleNames
            : NormalizeRoleNames(roles.Select(r => r.Name));

    private bool IsPrivilegedUser(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || _settings.EmailUsersAdmin.Length == 0)
            return false;

        var normalizedEmail = email.Trim();
        return _settings.EmailUsersAdmin.Any(a =>
            string.Equals(a?.Trim(), normalizedEmail, StringComparison.OrdinalIgnoreCase)
        );
    }

    private static IReadOnlyList<string> NormalizeRoleNames(IEnumerable<string> roleNames) =>
        [
            .. roleNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(GetRolePriority),
        ];

    private static int GetRolePriority(string roleName) =>
        roleName.ToUpperInvariant() switch
        {
            "ADMIN" => 0,
            "MANAGER" => 1,
            "USER" => 2,
            _ => 3,
        };

    private static SharedTokenPayload BuildTokenPayload(Guid userId, string email, IReadOnlyList<string> roleNames)
    {
        var normalized = NormalizeRoleNames(roleNames);
        return new SharedTokenPayload(
            email,
            normalized.FirstOrDefault() ?? DefaultRoleName,
            new Dictionary<string, object?>
            {
                ["roles"] = normalized.ToArray(),
                ["user_id"] = userId.ToString(),
            }
        );
    }

    public async Task LogoutAsync(string tokenJti, CancellationToken ct = default)
    {
        _logger.LogInformation("Revoking token jti {Jti}", tokenJti);

        try
        {
            if (string.IsNullOrWhiteSpace(tokenJti))
                throw LogicException.NullValue(nameof(tokenJti));

            await _authExtension.AddToBlacklistAsync(tokenJti).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao revogar token jti {Jti}.", tokenJti);
            throw;
        }
    }

    public async Task<UserResponse?> GetMeAsync(string email, CancellationToken ct = default)
    {
        _logger.LogInformation("Obtendo usuário autenticado {Email}", email);

        try
        {
            if (string.IsNullOrWhiteSpace(email))
                throw LogicException.NullValue(nameof(email));

            var user = await _repository.GetUserByEmailAsync(email, ct).ConfigureAwait(false);
            return user is null
                ? null
                : new UserResponse(
                    user.Email,
                    user.FullName,
                    ResolveUserRoleNames(email, user.Roles)
                );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter usuário autenticado {Email}.", email);
            throw;
        }
    }

    private static string GetRequiredClaim(ClaimsPrincipal p, params string[] types) =>
        GetOptionalClaim(p, types) ?? throw LogicException.NullValue(types[0]);

    private static string? GetOptionalClaim(ClaimsPrincipal p, params string[] types) =>
        types.Select(p.FindFirstValue).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
