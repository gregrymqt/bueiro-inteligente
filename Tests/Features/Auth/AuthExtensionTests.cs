using backend.Core.Settings;
using backend.Extensions.Auth;
using backend.Extensions.Auth.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SharedTokenPayload = backend.Extensions.Auth.Models.TokenPayload;
using SharedUserTokenData = backend.Extensions.Auth.Models.UserTokenData;

namespace backend.Tests.Features.Auth;

public sealed class AuthExtensionTests
{
    private readonly AuthExtension _authExtension = new(
        Options.Create(CreateJwtSettings()),
        Options.Create(CreateIotSettings()),
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

    private static JwtSettings CreateJwtSettings() =>
        new()
        {
            SecretKey = "secret-key",
            Algorithm = "HS256",
            AccessTokenExpireMinutes = 30,
        };

    private static IotSettings CreateIotSettings() =>
        new()
        {
            HardwareToken = "hardware-token",
        };
}