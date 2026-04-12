using HomeDomain = backend.Features.Home.Domain;

namespace backend.Tests.Features.Home;

public sealed class HomeServiceTests
{
    private readonly Mock<IHomeRepository> _repositoryMock = new(MockBehavior.Strict);
    private readonly HomeService _service;

    public HomeServiceTests()
    {
        _service = new HomeService(_repositoryMock.Object, Mock.Of<ILogger<HomeService>>());
    }

    [Fact]
    public async Task GetHomeContentAsync_ComConteudoExistente_DeveRetornarPayloadUnificado()
    {
        // Arrange
        Guid carouselId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        Guid statCardId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        HomeDomain.HomeContent content = new(
            [
                new HomeDomain.CarouselModel
                {
                    Id = carouselId,
                    Title = "Banner Principal",
                    Subtitle = "Subtítulo principal",
                    ImageUrl = "https://cdn.example.com/banner.jpg",
                    ActionUrl = "https://example.com/acao",
                    Order = 2,
                    Section = HomeDomain.CarouselSection.hero,
                }
            ],
            [
                new HomeDomain.StatCardModel
                {
                    Id = statCardId,
                    Title = "Alertas ativos",
                    Value = "12",
                    Description = "Ocorrências em monitoramento",
                    IconName = "bell",
                    Color = HomeDomain.StatCardColor.warning,
                    Order = 1,
                }
            ]
        );

        _repositoryMock
            .Setup(repository => repository.GetAllContentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        // Act
        HomeResponseDto result = await _service.GetHomeContentAsync();

        // Assert
        result.Should().BeEquivalentTo(
            new HomeResponseDto(
                [
                    new CarouselResponseDto(
                        carouselId,
                        "Banner Principal",
                        "Subtítulo principal",
                        "https://cdn.example.com/banner.jpg",
                        "https://example.com/acao",
                        2,
                        CarouselSection.hero
                    )
                ],
                [
                    new StatCardResponseDto(
                        statCardId,
                        "Alertas ativos",
                        "12",
                        "Ocorrências em monitoramento",
                        "bell",
                        StatCardColor.warning,
                        1
                    )
                ]
            )
        );

        _repositoryMock.Verify(repository => repository.GetAllContentAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateCarouselAsync_ComDadosValidos_DeveCriarERetornarCarousel()
    {
        // Arrange
        CarouselCreateDto request = new(
            " Banner Novo ",
            " Subtítulo Novo ",
            " https://cdn.example.com/banner-novo.jpg ",
            " https://example.com/novo ",
            3,
            CarouselSection.alerts
        );

        HomeDomain.CarouselModel createdCarousel = new()
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Title = "Banner Novo",
            Subtitle = "Subtítulo Novo",
            ImageUrl = "https://cdn.example.com/banner-novo.jpg",
            ActionUrl = "https://example.com/novo",
            Order = 3,
            Section = HomeDomain.CarouselSection.alerts,
        };

        _repositoryMock
            .Setup(repository => repository.CreateCarouselAsync(
                It.Is<HomeDomain.CarouselModel>(carousel =>
                    carousel.Title == "Banner Novo"
                    && carousel.Subtitle == "Subtítulo Novo"
                    && carousel.ImageUrl == "https://cdn.example.com/banner-novo.jpg"
                    && carousel.ActionUrl == "https://example.com/novo"
                    && carousel.Order == 3
                    && carousel.Section == HomeDomain.CarouselSection.alerts),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCarousel);

        // Act
        CarouselResponseDto result = await _service.CreateCarouselAsync(request);

        // Assert
        result.Should().BeEquivalentTo(
            new CarouselResponseDto(
                createdCarousel.Id,
                createdCarousel.Title,
                createdCarousel.Subtitle,
                createdCarousel.ImageUrl,
                createdCarousel.ActionUrl,
                createdCarousel.Order,
                request.Section
            )
        );

        _repositoryMock.Verify(repository => repository.CreateCarouselAsync(It.IsAny<HomeDomain.CarouselModel>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateStatCardAsync_ComDadosValidos_DeveAtualizarERetornarCard()
    {
        // Arrange
        Guid statCardId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        HomeDomain.StatCardModel existingStatCard = new()
        {
            Id = statCardId,
            Title = "Antigo",
            Value = "10",
            Description = "Descrição antiga",
            IconName = "bell",
            Color = HomeDomain.StatCardColor.success,
            Order = 2,
        };

        StatCardUpdateDto request = new(
            "Novo título",
            "11",
            "Nova descrição",
            "alert-circle",
            StatCardColor.danger,
            7
        );

        _repositoryMock
            .Setup(repository => repository.GetStatCardByIdAsync(statCardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStatCard);

        _repositoryMock
            .Setup(repository => repository.UpdateStatCardAsync(
                It.Is<HomeDomain.StatCardModel>(statCard =>
                    statCard.Id == statCardId
                    && statCard.Title == request.Title
                    && statCard.Value == request.Value
                    && statCard.Description == request.Description
                    && statCard.IconName == request.IconName
                    && statCard.Color == HomeDomain.StatCardColor.danger
                    && statCard.Order == request.Order),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStatCard);

        // Act
        StatCardResponseDto result = await _service.UpdateStatCardAsync(statCardId, request);

        // Assert
        result.Should().BeEquivalentTo(
            new StatCardResponseDto(
                statCardId,
                request.Title!,
                request.Value!,
                request.Description!,
                request.IconName!,
                request.Color!.Value,
                request.Order!.Value
            )
        );

        _repositoryMock.Verify(repository => repository.GetStatCardByIdAsync(statCardId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repository => repository.UpdateStatCardAsync(It.IsAny<HomeDomain.StatCardModel>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteCarouselAsync_ComCarouselInexistente_DeveLancarNotFoundException()
    {
        // Arrange
        Guid carouselId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        _repositoryMock
            .Setup(repository => repository.GetCarouselByIdAsync(carouselId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HomeDomain.CarouselModel?)null);

        // Act
        Func<Task> act = () => _service.DeleteCarouselAsync(carouselId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{carouselId}*");

        _repositoryMock.Verify(repository => repository.GetCarouselByIdAsync(carouselId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repository => repository.DeleteCarouselAsync(It.IsAny<HomeDomain.CarouselModel>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }
}