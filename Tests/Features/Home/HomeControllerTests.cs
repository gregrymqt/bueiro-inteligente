namespace backend.Tests.Features.Home;

public sealed class HomeControllerTests
{
    private readonly Mock<IHomeService> _homeServiceMock = new(MockBehavior.Strict);
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _controller = new HomeController(_homeServiceMock.Object);
    }

    [Fact]
    public async Task GetHomeContent_ComSucesso_DeveRetornarOk()
    {
        // Arrange
        HomeResponseDto response = new(
            [
                new CarouselResponseDto(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "Banner Principal",
                    "Subtítulo principal",
                    "https://cdn.example.com/banner.jpg",
                    "https://example.com/acao",
                    2,
                    CarouselSection.hero
                )
            ],
            []
        );

        _homeServiceMock
            .Setup(service => service.GetHomeContentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        ActionResult<HomeResponseDto> result = await _controller.GetHomeContent(CancellationToken.None);

        // Assert
        OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(response);

        _homeServiceMock.Verify(service => service.GetHomeContentAsync(It.IsAny<CancellationToken>()), Times.Once);
        _homeServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetHomeContent_ComConnectionException_DeveRetornarServiceUnavailable()
    {
        // Arrange
        _homeServiceMock
            .Setup(service => service.GetHomeContentAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConnectionException("PostgreSQL", "Falha ao consultar home."));

        // Act
        ActionResult<HomeResponseDto> result = await _controller.GetHomeContent(CancellationToken.None);

        // Assert
        ObjectResult objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        ProblemDetails problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        problemDetails.Status.Should().Be(StatusCodes.Status503ServiceUnavailable);

        _homeServiceMock.Verify(service => service.GetHomeContentAsync(It.IsAny<CancellationToken>()), Times.Once);
        _homeServiceMock.VerifyNoOtherCalls();
    }
}