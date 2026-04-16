using backend.Core;
using backend.Extensions.Auth;
using backend.Extensions.Auth.Abstractions;
using Microsoft.AspNetCore.Identity;
using SharedTokenPayload = backend.Extensions.Auth.Models.TokenPayload;
using SharedUserTokenData = backend.Extensions.Auth.Models.UserTokenData;

namespace backend.Tests.Features.Auth;

public sealed class AuthExtensionTests
{
    private readonly AuthExtension _authExtension = new(
        CreateSettings(),
        Mock.Of<ITokenBlacklistStore>(),
        Mock.Of<IPasswordHasher<object>>(),
        Mock.Of<ILogger<AuthExtension>>()
    );

    [Fact]
    public async Task CreateAccessToken_ComRolesMultiples_DevePreservarListaDeRoles()
    {
        // Arrange
        SharedTokenPayload payload = new(
            "user@example.com",
            "Admin",
            new Dictionary<string, object?>
            {
                ["roles"] = new[] { "Admin", "Manager", "User" },
            }
        );

        // Act
        string token = _authExtension.CreateAccessToken(payload);
        SharedUserTokenData currentUser = await _authExtension.GetCurrentUserAsync(token);

        // Assert
        currentUser.Email.Should().Be(payload.Email);
        currentUser.Jti.Should().NotBeNullOrWhiteSpace();
        currentUser.Roles.Should().Equal(new[] { "Admin", "Manager", "User" });
        currentUser.Role.Should().Be("Admin");
    }

    private static AppSettings CreateSettings()
    {
        return new AppSettings(
            "Bueiro Inteligente",
            "1.0.0",
            "/api/v1",
            "secret-key",
            "HS256",
            30,
            "hardware-token",
            "redis://localhost:6379",
            true,
            true,
            "postgres://cloud",
            "postgres://local",
            "postgres://local",
            "migrations",
            "rows-api-key",
            "https://api.rows.com/v1",
            "spreadsheet-id",
            "table-id",
            "google-client-id",
            "google-client-secret",
            Array.Empty<string>(),
            false,
            Array.Empty<string>()
        );
    }
}