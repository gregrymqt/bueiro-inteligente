using backend.Extensions.Auth.Logic;
using SharedUserTokenData = backend.Extensions.Auth.Models.UserTokenData;

namespace backend.Tests.Features.Auth;

public sealed class RoleCheckerTests
{
    [Fact]
    public async Task AuthorizeAsync_ComRolePermitida_DeveAceitarUsuario()
    {
        // Arrange
        RoleChecker checker = new(["Admin", "Manager"]);
        SharedUserTokenData currentUser = new("user@example.com", ["User", "Manager"], "jti-123");

        // Act
        SharedUserTokenData result = await checker.AuthorizeAsync(currentUser);

        // Assert
        result.Should().BeSameAs(currentUser);
        result.Roles.Should().Contain("Manager");
    }

    [Fact]
    public async Task AuthorizeAsync_ComStrictDbCheck_DeveAtualizarRolesDoUsuario()
    {
        // Arrange
        Mock<IUserRoleProvider> roleProviderMock = new(MockBehavior.Strict);
        roleProviderMock
            .Setup(provider =>
                provider.GetRolesByEmailAsync("user@example.com", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new[] { "Admin", "User" });

        RoleChecker checker = new(["Admin"], strictDbCheck: true, roleProviderMock.Object);
        SharedUserTokenData currentUser = new("user@example.com", ["User"], "jti-123");

        // Act
        SharedUserTokenData result = await checker.AuthorizeAsync(currentUser);

        // Assert
        result.Should().BeSameAs(currentUser);
        result.Roles.Should().Equal(new[] { "Admin", "User" });

        roleProviderMock.Verify(
            provider =>
                provider.GetRolesByEmailAsync("user@example.com", It.IsAny<CancellationToken>()),
            Times.Once
        );
        roleProviderMock.VerifyNoOtherCalls();
    }
}
