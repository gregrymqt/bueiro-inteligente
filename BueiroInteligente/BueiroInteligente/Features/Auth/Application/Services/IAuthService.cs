using BueiroInteligente.Features.Auth.Application.DTOs;

namespace BueiroInteligente.Features.Auth.Application.Services;

public interface IAuthService
{
    Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<UserResponse> RegisterAsync(UserCreateRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(string tokenJti, CancellationToken cancellationToken = default);

    Task<UserResponse?> GetMeAsync(string email, CancellationToken cancellationToken = default);
}