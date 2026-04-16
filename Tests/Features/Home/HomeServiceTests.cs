using HomeDomain = backend.Features.Home.Domain;

namespace backend.Tests.Features.Home;

public sealed class HomeServiceTests
{
    private readonly Mock<IHomeRepository> _repositoryMock = new(); // Loose por padrão
    private readonly HomeService _service;

    public HomeServiceTests()
    {
        _service = new HomeService(_repositoryMock.Object, Mock.Of<ILogger<HomeService>>());
    }

    #region Helpers (O padrão para o seu projeto)

    private HomeDomain.CarouselModel BuildCarousel(Guid? id = null, string title = "Banner") =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Title = title,
            Subtitle = "Subtítulo",
            ImageUrl = "https://example.com/img.jpg",
            ActionUrl = "https://example.com/action",
            Order = 1,
            Section = HomeDomain.CarouselSection.hero,
        };

    private HomeDomain.StatCardModel BuildStatCard(Guid? id = null, string title = "Stat") =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Title = title,
            Value = "10",
            Description = "Desc",
            IconName = "icon",
            Color = HomeDomain.StatCardColor.success,
            Order = 1,
        };

    #endregion

    [Fact]
    public async Task GetHomeContentAsync_DeveRetornarPayloadUnificado()
    {
        // Arrange
        var content = new HomeDomain.HomeContent([BuildCarousel()], [BuildStatCard()]);

        _repositoryMock
            .Setup(r => r.GetAllContentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        // Act
        var result = await _service.GetHomeContentAsync();

        // Assert
        result.Carousels.Should().HaveCount(1);
        result.Stats.Should().HaveCount(1);
        result.Carousels.First().Title.Should().Be(content.Carousels.First().Title);
    }

    [Fact]
    public async Task CreateCarouselAsync_DeveTratarEspacosETitleCase()
    {
        // Arrange
        var request = new CarouselCreateDto(
            " Banner Novo ",
            " Sub ",
            "url",
            "action",
            1,
            CarouselSection.alerts
        );
        var created = BuildCarousel(title: "Banner Novo");

        _repositoryMock
            .Setup(r =>
                r.CreateCarouselAsync(
                    It.IsAny<HomeDomain.CarouselModel>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(created);

        // Act
        var result = await _service.CreateCarouselAsync(request);

        // Assert
        result.Title.Should().Be("Banner Novo");
        _repositoryMock.Verify(
            r =>
                r.CreateCarouselAsync(
                    It.Is<HomeDomain.CarouselModel>(c => c.Title == "Banner Novo"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateStatCardAsync_Sucesso_DeveAtualizarCampos()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = BuildStatCard(id);
        var request = new StatCardUpdateDto(
            "Novo Título",
            "20",
            "Nova Desc",
            "new-icon",
            StatCardColor.danger,
            5
        );

        _repositoryMock
            .Setup(r => r.GetStatCardByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _repositoryMock
            .Setup(r =>
                r.UpdateStatCardAsync(
                    It.IsAny<HomeDomain.StatCardModel>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(existing);

        // Act
        var result = await _service.UpdateStatCardAsync(id, request);

        // Assert
        result.Title.Should().Be(request.Title);
        result.Color.Should().Be(StatCardColor.danger);
    }

    [Fact]
    public async Task DeleteCarouselAsync_Inexistente_DeveLancarNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetCarouselByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HomeDomain.CarouselModel?)null);

        // Act & Assert
        await _service
            .Invoking(s => s.DeleteCarouselAsync(id))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
