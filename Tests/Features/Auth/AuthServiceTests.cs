using backend.Core.Settings;
using backend.Features.Auth.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using SharedTokenPayload = backend.Extensions.Auth.Models.TokenPayload;

namespace backend.Tests.Features.Auth;

public sealed class AuthServiceTests
{
    private readonly GeneralSettings _settings;
    private readonly Mock<IAuthRepository> _repositoryMock = new(); // Default is Loose
    private readonly Mock<IAuthExtension> _authExtensionMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _settings = CreateSettings();
        _service = new AuthService(
            _repositoryMock.Object,
            _authExtensionMock.Object,
            _unitOfWorkMock.Object,
            Options.Create(_settings),
            Mock.Of<ILogger<AuthService>>()
        );
    }

    #region Helpers (O segredo para limpar o Arrange)

    private User CreateUser(string email, string roleName = "User") =>
        new()
        {
            Email = email,
            HashedPassword = "hashed-password",
            Roles = new List<Role> { new() { Name = roleName } },
        };

    private void SetupUser(string email, User? user) =>
        _repositoryMock
            .Setup(r => r.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    #endregion

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
    {
        // Arrange
        var request = new LoginRequest("user@example.com", "Senha123!");
        var user = CreateUser(request.Email, "Admin");

        SetupUser(request.Email, user);
        _authExtensionMock
            .Setup(e => e.VerifyPasswordAsync(request.Password, user.HashedPassword))
            .ReturnsAsync(true);
        _authExtensionMock
            .Setup(e => e.CreateAccessToken(It.IsAny<SharedTokenPayload>()))
            .Returns("jwt-token");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("jwt-token");
        _authExtensionMock.Verify(
            e => e.CreateAccessToken(It.IsAny<SharedTokenPayload>()),
            Times.Once
        );
    }

    [Theory]
    [InlineData("user@example.com", "SenhaErrada", false)]
    [InlineData("naoexiste@example.com", "Senha123!", null)]
    public async Task Login_CenariosInvalidos_DeveRetornarNull(
        string email,
        string password,
        bool? passCorrect
    )
    {
        // Arrange
        var request = new LoginRequest(email, password);
        var user = passCorrect.HasValue ? CreateUser(email) : null;

        SetupUser(email, user);
        if (user != null)
            _authExtensionMock
                .Setup(e => e.VerifyPasswordAsync(password, user.HashedPassword))
                .ReturnsAsync(passCorrect!.Value);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Register_ComEmailExistente_DeveLancarLogicException()
    {
        // Arrange
        var request = new UserCreateRequest("existente@example.com", "123", "Nome", "User");
        SetupUser(request.Email, CreateUser(request.Email));

        // Act & Assert
        await _service
            .Invoking(s => s.RegisterAsync(request))
            .Should()
            .ThrowAsync<LogicException>()
            .WithMessage($"*{request.Email}*");
    }

    [Fact]
    public async Task Logout_DeveAdicionarNaBlacklist()
    {
        // Act
        await _service.LogoutAsync("jti-123");

        // Assert
        _authExtensionMock.Verify(e => e.AddToBlacklistAsync("jti-123"), Times.Once);
    }

    private static ClaimsPrincipal BuildGooglePrincipal(
        string googleId,
        string email,
        string fullName,
        string avatarUrl
    )
    {
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, googleId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, fullName),
            new Claim("picture", avatarUrl),
        };

        ClaimsIdentity identity = new(claims, "Google");

        return new ClaimsPrincipal(identity);
    }

    private static GeneralSettings CreateSettings(params string[] emailUsersAdmin) =>
        new()
        {
            ProjectName = "Bueiro Inteligente",
            Version = "1.0.0",
            ApiStr = "/api/v1",
            AllowedHosts = "*",
            AllowedOrigins = [],
            EmailUsersAdmin = emailUsersAdmin,
            AppIdSecret = string.Empty,
        };
}
