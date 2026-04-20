namespace backend.Tests.Features.Home;

public sealed class HomeControllerTests
{
    private readonly Mock<IHomeService> _homeServiceMock = new(); // Mock Loose por padrão
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _controller = new HomeController(_homeServiceMock.Object);
    }

    #region Helpers (Gabarito para o seu agente)

    private HomeResponseDto BuildHomeResponse() =>
        new(
            Carousels:
            [
                new CarouselResponseDto(
                    Guid.NewGuid(),
                    "Banner",
                    "Sub",
                    "url",
                    "action",
                    1,
                    CarouselSection.hero
                ),
            ],
            Stats: []
        );

    #endregion

    [Fact]
    public async Task GetHomeContent_Sucesso_DeveRetornarOkComDados()
    {
        // Arrange
        var expectedResponse = BuildHomeResponse();
        _homeServiceMock
            .Setup(s => s.GetHomeContentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetHomeContent(default);

        // Assert
        result
            .Result.Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetHomeContent_ErroDeConexao_DevePropagarConnectionException()
    {
        // Arrange
        _homeServiceMock
            .Setup(s => s.GetHomeContentAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConnectionException("Database", "Falha crítica"));

        // Act
        Func<Task> act = () => _controller.GetHomeContent(default);

        // Assert
        await act.Should().ThrowAsync<ConnectionException>();
    }
}
