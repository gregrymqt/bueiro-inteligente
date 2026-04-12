using SharedTokenPayload = backend.Extensions.TokenPayload;

namespace backend.Tests.Features.Auth;

public sealed class AuthServiceTests
{
    private readonly Mock<IAuthRepository> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IAuthExtension> _authExtensionMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);

    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(
            _repositoryMock.Object,
            _authExtensionMock.Object,
            _unitOfWorkMock.Object,
            Mock.Of<ILogger<AuthService>>()
        );
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
    {
        // Arrange
        LoginRequest request = new("user@example.com", "Senha123!");
        Role role = new() { Name = "Admin" };
        User user = new()
        {
            Email = request.Email,
            HashedPassword = "hashed-password",
            Role = role,
            RoleId = role.Id,
        };

        _repositoryMock
            .Setup(repository => repository.GetUserByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authExtensionMock
            .Setup(extension => extension.VerifyPasswordAsync(request.Password, user.HashedPassword))
            .ReturnsAsync(true);

        _authExtensionMock
            .Setup(extension => extension.CreateAccessToken(
                It.Is<SharedTokenPayload>(payload =>
                    payload.Email == request.Email && payload.Role == role.Name)))
            .Returns("jwt-token");

        // Act
        TokenResponse? result = await _service.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new TokenResponse("jwt-token", "bearer"));

        _repositoryMock.Verify(repository => repository.GetUserByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _authExtensionMock.Verify(extension => extension.VerifyPasswordAsync(request.Password, user.HashedPassword), Times.Once);
        _authExtensionMock.Verify(extension => extension.CreateAccessToken(It.IsAny<SharedTokenPayload>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _authExtensionMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornarNull()
    {
        // Arrange
        LoginRequest request = new("user@example.com", "Senha123!");
        Role role = new() { Name = "User" };
        User user = new()
        {
            Email = request.Email,
            HashedPassword = "hashed-password",
            Role = role,
            RoleId = role.Id,
        };

        _repositoryMock
            .Setup(repository => repository.GetUserByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authExtensionMock
            .Setup(extension => extension.VerifyPasswordAsync(request.Password, user.HashedPassword))
            .ReturnsAsync(false);

        // Act
        TokenResponse? result = await _service.LoginAsync(request);

        // Assert
        result.Should().BeNull();

        _repositoryMock.Verify(repository => repository.GetUserByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _authExtensionMock.Verify(extension => extension.VerifyPasswordAsync(request.Password, user.HashedPassword), Times.Once);
        _authExtensionMock.Verify(extension => extension.CreateAccessToken(It.IsAny<SharedTokenPayload>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
        _authExtensionMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Register_ComDadosValidos_DeveCriarUsuario()
    {
        // Arrange
        UserCreateRequest request = new("user@example.com", "Senha123!", "User Test", "Admin");
        Role role = new() { Name = "Admin" };

        _repositoryMock
            .Setup(repository => repository.GetUserByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _repositoryMock
            .Setup(repository => repository.GetRoleByNameAsync(request.Role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _authExtensionMock
            .Setup(extension => extension.GetPasswordHashAsync(request.Password))
            .ReturnsAsync("hashed-password");

        _repositoryMock
            .Setup(repository => repository.AddUserAsync(
                It.Is<User>(user =>
                    user.Email == request.Email
                    && user.FullName == request.FullName
                    && user.HashedPassword == "hashed-password"
                    && user.RoleId == role.Id
                    && user.Role == role),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        UserResponse result = await _service.RegisterAsync(request);

        // Assert
        result.Should().BeEquivalentTo(new UserResponse(request.Email, request.FullName, role.Name));

        _repositoryMock.Verify(repository => repository.GetUserByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repository => repository.GetRoleByNameAsync(request.Role, It.IsAny<CancellationToken>()), Times.Once);
        _authExtensionMock.Verify(extension => extension.GetPasswordHashAsync(request.Password), Times.Once);
        _repositoryMock.Verify(repository => repository.AddUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _authExtensionMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Register_ComEmailExistente_DeveLancarLogicException()
    {
        // Arrange
        UserCreateRequest request = new("user@example.com", "Senha123!", "User Test", "Admin");
        User existingUser = new()
        {
            Email = request.Email,
            HashedPassword = "hashed-password",
        };

        _repositoryMock
            .Setup(repository => repository.GetUserByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = () => _service.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<LogicException>()
            .WithMessage($"*{request.Email}*");

        _repositoryMock.Verify(repository => repository.GetUserByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repository => repository.GetRoleByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _authExtensionMock.Verify(extension => extension.GetPasswordHashAsync(It.IsAny<string>()), Times.Never);
        _repositoryMock.Verify(repository => repository.AddUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
        _authExtensionMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Logout_ComJtiValido_DeveAdicionarNaBlacklist()
    {
        // Arrange
        string tokenJti = "jti-123";

        _authExtensionMock
            .Setup(extension => extension.AddToBlacklistAsync(tokenJti))
            .Returns(Task.CompletedTask);

        // Act
        await _service.LogoutAsync(tokenJti);

        // Assert
        _authExtensionMock.Verify(extension => extension.AddToBlacklistAsync(tokenJti), Times.Once);
        _authExtensionMock.VerifyNoOtherCalls();
        _repositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Logout_ComJtiVazio_DeveLancarLogicException()
    {
        // Arrange
        Func<Task> act = () => _service.LogoutAsync("   ");

        // Act / Assert
        await act.Should().ThrowAsync<LogicException>()
            .WithMessage("*tokenJti*");

        _authExtensionMock.Verify(extension => extension.AddToBlacklistAsync(It.IsAny<string>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
        _authExtensionMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }
}