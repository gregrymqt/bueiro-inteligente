namespace backend.Tests.Features.Drains;

public sealed class DrainServiceTests
{
    private readonly Mock<IDrainRepository> _repositoryMock = new(); // Loose por padrão
    private readonly DrainService _service;

    public DrainServiceTests()
    {
        _service = new DrainService(_repositoryMock.Object, Mock.Of<ILogger<DrainService>>());
    }

    #region Helpers (O "Manual" para o seu agente)

    private DrainEntity BuildEntity(Guid? id = null, string hwId = "HW-01") =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Bueiro Teste",
            Address = "Rua Teste",
            HardwareId = hwId,
            Latitude = -23.0,
            Longitude = -46.0,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };

    #endregion

    [Fact]
    public async Task GetAllDrainsAsync_DeveRetornarDrainsMapeados()
    {
        // Arrange
        var entities = new[] { BuildEntity(), BuildEntity() };
        _repositoryMock
            .Setup(r => r.GetAllAsync(0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        var result = await _service.GetAllDrainsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be(entities[0].Name);
    }

    [Fact]
    public async Task CreateDrainAsync_ComHardwareDuplicado_DeveLancarLogicException()
    {
        // Arrange
        var request = new DrainCreateRequest("Novo", "Rua", 0, 0, "HW-DUPLICADO", true);
        _repositoryMock
            .Setup(r => r.GetByHardwareIdAsync(request.HardwareId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildEntity(hwId: request.HardwareId));

        // Act & Assert
        await _service
            .Invoking(s => s.CreateDrainAsync(request))
            .Should()
            .ThrowAsync<LogicException>()
            .WithMessage($"*{request.HardwareId}*");

        _repositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<DrainEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Theory]
    [InlineData("NotFound")]
    [InlineData("HardwareConflict")]
    public async Task UpdateDrainAsync_CenariosDeErro_DeveLancarExcecaoCorreta(string erroTipo)
    {
        // Arrange
        var drainId = Guid.NewGuid();
        var request = new DrainUpdateRequest("Update", "Rua", 0, 0, true, "HW-NOVO");

        if (erroTipo == "NotFound")
            _repositoryMock
                .Setup(r => r.GetByIdAsync(drainId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DrainEntity?)null);
        else
        {
            _repositoryMock
                .Setup(r => r.GetByIdAsync(drainId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildEntity(drainId));
            _repositoryMock
                .Setup(r => r.GetByHardwareIdAsync("HW-NOVO", It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildEntity(Guid.NewGuid(), "HW-NOVO")); // Outro bueiro já usa esse HW
        }

        // Act & Assert
        var expectedException =
            erroTipo == "NotFound" ? typeof(NotFoundException) : typeof(LogicException);
        await _service
            .Invoking(s => s.UpdateDrainAsync(drainId, request))
            .Should()
            .ThrowAsync<Exception>()
            .Where(e => e.GetType() == expectedException);
    }

    [Fact]
    public async Task DeleteDrainAsync_Sucesso_DeveChamarDeleteNoRepositorio()
    {
        // Arrange
        var entity = BuildEntity();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        await _service.DeleteDrainAsync(entity.Id);

        // Assert
        _repositoryMock.Verify(
            r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
