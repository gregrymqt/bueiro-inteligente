namespace backend.Tests.Features.Auth;

public sealed class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new(MockBehavior.Strict);

    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarOk()
    {
        // Arrange
        LoginRequest request = new("user@example.com", "Senha123!");
        TokenResponse token = new("jwt-token", "bearer");

        _authServiceMock
            .Setup(service => service.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        ActionResult<TokenResponse> result = await _controller.Login(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(token);

        _authServiceMock.Verify(service => service.LoginAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _authServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
    {
        // Arrange
        LoginRequest request = new("user@example.com", "Senha123!");

        _authServiceMock
            .Setup(service => service.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenResponse?)null);

        // Act
        ActionResult<TokenResponse> result = await _controller.Login(request, CancellationToken.None);

        // Assert
        UnauthorizedObjectResult unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        ProblemDetails problemDetails = unauthorizedResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        problemDetails.Status.Should().Be(StatusCodes.Status401Unauthorized);
        problemDetails.Detail.Should().Be("Incorrect email or password.");

        _authServiceMock.Verify(service => service.LoginAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _authServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_ComLogicException_DeveRetornarBadRequest()
    {
        // Arrange
        LoginRequest request = new("user@example.com", "Senha123!");

        _authServiceMock
            .Setup(service => service.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LogicException("Dados inválidos."));

        // Act
        ActionResult<TokenResponse> result = await _controller.Login(request, CancellationToken.None);

        // Assert
        BadRequestObjectResult badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        ProblemDetails problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Detail.Should().Be("Dados inválidos.");

        _authServiceMock.Verify(service => service.LoginAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _authServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Register_ComDadosValidos_DeveRetornarCreated()
    {
        // Arrange
        UserCreateRequest request = new("user@example.com", "Senha123!", "User Test", "Admin");
        UserResponse response = new(request.Email, request.FullName, request.Role);

        _authServiceMock
            .Setup(service => service.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        ActionResult<UserResponse> result = await _controller.Register(request, CancellationToken.None);

        // Assert
        CreatedResult createdResult = result.Result.Should().BeOfType<CreatedResult>().Subject;

        createdResult.Location.Should().Be("/auth/me");
        createdResult.Value.Should().BeEquivalentTo(response);

        _authServiceMock.Verify(service => service.RegisterAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _authServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Register_ComLogicException_DeveRetornarBadRequest()
    {
        // Arrange
        UserCreateRequest request = new("user@example.com", "Senha123!", "User Test", "Admin");

        _authServiceMock
            .Setup(service => service.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LogicException("Email já cadastrado."));

        // Act
        ActionResult<UserResponse> result = await _controller.Register(request, CancellationToken.None);

        // Assert
        BadRequestObjectResult badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        ProblemDetails problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Detail.Should().Be("Email já cadastrado.");

        _authServiceMock.Verify(service => service.RegisterAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _authServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMe_ComEmailNosClaims_DeveChamarServicoComEmailExtraido()
    {
        // Arrange
        string email = "user@example.com";
        UserResponse response = new(email, "User Test", "Admin");
        _controller.ControllerContext = BuildControllerContext(new Claim(ClaimTypes.Email, email));

        _authServiceMock
            .Setup(service => service.GetMeAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        ActionResult<UserResponse> result = await _controller.GetMe(CancellationToken.None);

        // Assert
        OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(response);

        _authServiceMock.Verify(service => service.GetMeAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        _authServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMe_SemEmailNosClaims_DeveRetornarBadRequest()
    {
        // Arrange
        _controller.ControllerContext = BuildControllerContext();

        // Act
        ActionResult<UserResponse> result = await _controller.GetMe(CancellationToken.None);

        // Assert
        BadRequestObjectResult badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        ProblemDetails problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Detail.Should().Contain("email");

        _authServiceMock.Verify(service => service.GetMeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _authServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Logout_ComJtiNosClaims_DeveChamarServicoComJtiExtraido()
    {
        // Arrange
        string tokenJti = "jti-123";
        _controller.ControllerContext = BuildControllerContext(new Claim("jti", tokenJti));

        _authServiceMock
            .Setup(service => service.LogoutAsync(tokenJti, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await _controller.Logout(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _authServiceMock.Verify(service => service.LogoutAsync(tokenJti, It.IsAny<CancellationToken>()), Times.Once);
        _authServiceMock.VerifyNoOtherCalls();
    }

    private static ControllerContext BuildControllerContext(params Claim[] claims)
    {
        ClaimsIdentity identity = new(claims, "TestAuth");
        ClaimsPrincipal principal = new(identity);

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal,
            },
        };
    }
}