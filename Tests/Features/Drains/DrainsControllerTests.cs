namespace backend.Tests.Features.Drains;

public sealed class DrainsControllerTests
{
    private readonly Mock<IDrainService> _drainServiceMock = new(); // Loose por padrão
    private readonly DrainsController _controller;

    public DrainsControllerTests()
    {
        _controller = new DrainsController(_drainServiceMock.Object);
    }

    #region Helpers (Molde para o seu agente)

    private DrainResponse CreateResponse(Guid? id = null, string name = "Bueiro Teste") =>
        new(
            id ?? Guid.NewGuid(),
            name,
            "Rua Teste, 123",
            -23.9,
            -46.3,
            true,
            "HW-TEST",
            DateTimeOffset.UtcNow
        );

    #endregion

    [Fact]
    public async Task GetAll_DeveRetornarListaDeBueiros()
    {
        // Arrange
        var drains = new List<DrainResponse> { CreateResponse(), CreateResponse() };
        _drainServiceMock
            .Setup(s => s.GetAllDrainsAsync(0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(drains);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result
            .Result.Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(drains);
    }

    [Fact]
    public async Task Create_ComSucesso_DeveRetornarCreated()
    {
        // Arrange
        var request = new DrainCreateRequest("Novo", "Endereco", 0, 0, "HW-01", true);
        var response = CreateResponse(name: request.Name);

        _drainServiceMock
            .Setup(s => s.CreateDrainAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Create(request, default);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(DrainsController.GetById));
        createdResult.Value.Should().BeEquivalentTo(response);
    }

    [Theory]
    [InlineData("NotFound")]
    [InlineData("LogicException")]
    public async Task Drains_TratamentoDeErros_DevePropagarExcecao(string erroTipo)
    {
        // Arrange
        var drainId = Guid.NewGuid();
        if (erroTipo == "NotFound")
            _drainServiceMock
                .Setup(s => s.GetDrainByIdAsync(drainId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("Drain", drainId));
        else
            _drainServiceMock
                .Setup(s => s.GetDrainByIdAsync(drainId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new LogicException("Erro de validação"));

        // Act
        Func<Task> act = () => _controller.GetById(drainId);

        // Assert
        if (erroTipo == "NotFound")
            await act.Should().ThrowAsync<NotFoundException>();
        else
            await act.Should().ThrowAsync<LogicException>();
    }

    [Fact]
    public async Task Delete_DeveRetornarNoContent()
    {
        // Act
        var result = await _controller.Delete(Guid.NewGuid(), default);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
