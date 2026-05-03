using backend.Core.Settings;
using backend.Features.Auth.Application.Interfaces;
using backend.Features.Auth.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace backend.Tests.Features.Auth;

public sealed class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<IAuthenticationService> _authInternalMock = new();
    private readonly IOptions<GoogleSettings> _googleSettings = Options.Create(
        new GoogleSettings
        {
            GoogleClientId = "google-client-id",
            GoogleClientSecret = "google-client-secret",
            GoogleFrontendRedirectUrl = "https://frontend.example",
            AllowedOrigins = ["http://localhost:5173"],
        }
    );

    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_authServiceMock.Object, _googleSettings);
    }

    #region Helpers de Infraestrutura

    private void SetupControllerContext(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };

        // Necessário para testes de Google Callback que acessam RequestServices
        var services = new ServiceCollection();
        services.AddSingleton(_authInternalMock.Object);
        _controller.HttpContext.RequestServices = services.BuildServiceProvider();
    }

    #endregion

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarOk()
    {
        // Arrange
        var request = new LoginRequest("user@example.com", "Senha123!");
        var expectedToken = new TokenResponse("jwt-token", "bearer");

        _authServiceMock
            .Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _controller.Login(request, default);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedToken);
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("erro@example.com", "123");

        _authServiceMock
            .Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenResponse?)null);

        // Act
        var result = await _controller.Login(request, default);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Login_QuandoServiceLancaLogicException_DevePropagarExcecao()
    {
        // Arrange
        var request = new LoginRequest("erro@example.com", "123");

        _authServiceMock
            .Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LogicException("Dados inválidos."));

        // Act
        Func<Task> act = () => _controller.Login(request, default);

        // Assert
        await act.Should().ThrowAsync<LogicException>();
    }

    [Fact]
    public async Task GetMe_DeveExtrairEmailDosClaimsERetornarUsuario()
    {
        // Arrange
        string email = "user@example.com";
        var expectedResponse = new UserResponse(email, "User Test", ["Admin"]);
        SetupControllerContext(new Claim(ClaimTypes.Email, email));

        _authServiceMock
            .Setup(s => s.GetMeAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetMe(default);

        // Assert
        result
            .Result.Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void GoogleLogin_DeveRetornarChallengeConfigurado()
    {
        // Act
        var result = _controller.GoogleLogin("http://localhost:5173");

        // Assert
        var challenge = result.Should().BeOfType<ChallengeResult>().Subject;
        challenge.AuthenticationSchemes.Should().Contain(GoogleDefaults.AuthenticationScheme);
        challenge.Properties!.RedirectUri.Should().Contain("frontend_redirect=");
    }

    [Fact]
    public async Task GoogleCallback_Sucesso_DeveRedirecionarComToken()
    {
        // Arrange
        SetupControllerContext(new Claim(ClaimTypes.NameIdentifier, "123"));

        var authResult = AuthenticateResult.Success(
            new AuthenticationTicket(_controller.User, IdentityConstants.ExternalScheme)
        );
        _authInternalMock
            .Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(authResult);

        _authServiceMock
            .Setup(s =>
                s.SignInWithGoogleAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<HttpContext>())
            )
            .ReturnsAsync("jwt-token");

        // Act
        var result = await _controller.GoogleCallback("http://localhost:5173", default);

        // Assert
        result
            .Should()
            .BeOfType<RedirectResult>()
            .Which.Url.Should()
            .Be("http://localhost:5173#token=jwt-token");
    }
}
