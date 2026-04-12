namespace backend.Tests.Features.Home;

public sealed class HomeAdminControllerTests
{
    private readonly Mock<IHomeService> _homeServiceMock = new(MockBehavior.Strict);
    private readonly HomeAdminController _controller;

    public HomeAdminControllerTests()
    {
        _controller = new HomeAdminController(_homeServiceMock.Object);
    }

    [Fact]
    public async Task CreateCarousel_ComSucesso_DeveRetornarCreatedAtAction()
    {
        // Arrange
        CarouselCreateDto request = new(
            "Banner Novo",
            "Subtítulo Novo",
            "https://cdn.example.com/banner-novo.jpg",
            "https://example.com/novo",
            3,
            CarouselSection.alerts
        );

        CarouselResponseDto response = new(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            request.Title,
            request.Subtitle,
            request.ImageUrl,
            request.ActionUrl,
            request.Order,
            request.Section
        );

        _homeServiceMock
            .Setup(service => service.CreateCarouselAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        ActionResult<CarouselResponseDto> result = await _controller.CreateCarousel(request, CancellationToken.None);

        // Assert
        CreatedAtActionResult createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(HomeAdminController.GetCarouselById));
        createdResult.RouteValues.Should().ContainKey("carouselId").WhoseValue.Should().Be(response.Id);
        createdResult.Value.Should().BeEquivalentTo(response);

        _homeServiceMock.Verify(service => service.CreateCarouselAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _homeServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateStatCard_ComSucesso_DeveRetornarOk()
    {
        // Arrange
        Guid statCardId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        StatCardUpdateDto request = new(
            "Novo título",
            "11",
            "Nova descrição",
            "alert-circle",
            StatCardColor.danger,
            7
        );

        StatCardResponseDto response = new(
            statCardId,
            request.Title!,
            request.Value!,
            request.Description!,
            request.IconName!,
            request.Color!.Value,
            request.Order!.Value
        );

        _homeServiceMock
            .Setup(service => service.UpdateStatCardAsync(statCardId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        ActionResult<StatCardResponseDto> result = await _controller.UpdateStatCard(statCardId, request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(response);

        _homeServiceMock.Verify(service => service.UpdateStatCardAsync(statCardId, request, It.IsAny<CancellationToken>()), Times.Once);
        _homeServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteStatCard_ComSucesso_DeveRetornarNoContent()
    {
        // Arrange
        Guid statCardId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        _homeServiceMock
            .Setup(service => service.DeleteStatCardAsync(statCardId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await _controller.DeleteStatCard(statCardId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _homeServiceMock.Verify(service => service.DeleteStatCardAsync(statCardId, It.IsAny<CancellationToken>()), Times.Once);
        _homeServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateCarousel_ComLogicException_DeveRetornarBadRequest()
    {
        // Arrange
        Guid carouselId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        CarouselUpdateDto request = new(
            "Título",
            null,
            null,
            null,
            null,
            null
        );

        _homeServiceMock
            .Setup(service => service.UpdateCarouselAsync(carouselId, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LogicException("Dados inválidos."));

        // Act
        ActionResult<CarouselResponseDto> result = await _controller.UpdateCarousel(carouselId, request, CancellationToken.None);

        // Assert
        ObjectResult objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        ProblemDetails problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Detail.Should().Contain("Dados inválidos");

        _homeServiceMock.Verify(service => service.UpdateCarouselAsync(carouselId, request, It.IsAny<CancellationToken>()), Times.Once);
        _homeServiceMock.VerifyNoOtherCalls();
    }
}