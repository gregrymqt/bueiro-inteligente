namespace backend.Tests.Features.Home;

public sealed class HomeAdminControllerTests
{
    private readonly Mock<IHomeService> _homeServiceMock = new(); // Loose por padrão
    private readonly HomeAdminController _controller;

    public HomeAdminControllerTests()
    {
        _controller = new HomeAdminController(_homeServiceMock.Object);
    }

    #region Helpers (Padrão para o seu agente)

    private CarouselResponseDto CreateCarouselResponse(Guid? id = null, string title = "Banner") =>
        new(
            id ?? Guid.NewGuid(),
            title,
            "Subtítulo",
            "https://example.com/img.jpg",
            "https://example.com/action",
            1,
            CarouselSection.alerts
        );

    private StatCardResponseDto CreateStatCardResponse(Guid? id = null, string title = "Stat") =>
        new(id ?? Guid.NewGuid(), title, "10", "Descrição", "icon", StatCardColor.success, 1);

    #endregion

    [Fact]
    public async Task CreateCarousel_ComSucesso_DeveRetornarCreatedAtAction()
    {
        // Arrange
        var request = new CarouselCreateDto(
            "Novo",
            "Sub",
            Guid.NewGuid(),
            "action",
            1,
            CarouselSection.alerts
        );
        var response = CreateCarouselResponse(title: request.Title);

        _homeServiceMock
            .Setup(s => s.CreateCarouselAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.CreateCarousel(request, default);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(HomeAdminController.GetCarouselById));
        createdResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task UpdateStatCard_ComSucesso_DeveRetornarOk()
    {
        // Arrange
        var statCardId = Guid.NewGuid();
        var request = new StatCardUpdateDto(
            "Título",
            "20",
            "Desc",
            "icon",
            StatCardColor.danger,
            2
        );
        var response = CreateStatCardResponse(statCardId, request.Title!);

        _homeServiceMock
            .Setup(s => s.UpdateStatCardAsync(statCardId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.UpdateStatCard(statCardId, request, default);

        // Assert
        result
            .Result.Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(response);
    }

    [Theory]
    [InlineData("LogicException")]
    [InlineData("NotFound")]
    public async Task HomeAdmin_TratamentoDeErros_DevePropagarExcecao(string erroTipo)
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new CarouselUpdateDto("Título", null, null, null, null, null);

        if (erroTipo == "LogicException")
            _homeServiceMock
                .Setup(s => s.UpdateCarouselAsync(id, request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new LogicException("Dados inválidos"));
        else
            _homeServiceMock
                .Setup(s => s.UpdateCarouselAsync(id, request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("Carousel", id));

        // Act
        Func<Task> act = () => _controller.UpdateCarousel(id, request, default);

        // Assert
        if (erroTipo == "LogicException")
            await act.Should().ThrowAsync<LogicException>();
        else
            await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteStatCard_DeveRetornarNoContent()
    {
        // Act
        var result = await _controller.DeleteStatCard(Guid.NewGuid(), default);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
