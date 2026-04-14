using System.Security.Claims;
using backend.Features.Auth.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace backend.Features.Auth.Application.Services;

public interface IAuthService
{
    Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<UserResponse> RegisterAsync(UserCreateRequest request, CancellationToken cancellationToken = default);

    Task<string> SignInWithGoogleAsync(ClaimsPrincipal principal, HttpContext httpContext);

    Task LogoutAsync(string tokenJti, CancellationToken cancellationToken = default);

    Task<UserResponse?> GetMeAsync(string email, CancellationToken cancellationToken = default);
}