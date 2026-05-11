using backend.extensions.Services.Realtime.Abstractions;
using backend.Features.Monitoring.Application.Interfaces;
using backend.Features.Monitoring.Domain.Configuration;

namespace backend.Tests.Features.Monitoring;

public sealed class MonitoringServiceTests
{
    private readonly Mock<IMonitoringRepository> _repositoryMock = new();
    private readonly Mock<IMonitoringIngestionService> _ingestionMock = new();
    private readonly Mock<IRealtimeService> _realtimeMock = new();
    private readonly MonitoringService _service;

    public MonitoringServiceTests()
    {
        _repositoryMock
            .Setup(r => r.GetConfigByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BueiroConfiguration
            {
                IdBueiro = "DRN-01",
                MaxHeight = 120.0,
                CriticalThreshold = 80.0,
                AlertThreshold = 50.0
            });

        _ingestionMock
            .Setup(i => i.SaveSensorDataAsync(It.IsAny<DrainStatusDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _service = new MonitoringService(
            _repositoryMock.Object,
            _ingestionMock.Object,
            _realtimeMock.Object,
            Mock.Of<ILogger<MonitoringService>>()
        );
    }

    #region Helpers (O gabarito definitivo)

    private SensorPayloadDTO CreatePayload(double distance) =>
        new("DRN-01", distance, -23.9, -46.3);

    private DrainStatusDTO CreateStatus(string id = "DRN-01", string status = "Normal") =>
        new(id, 90, 25, status, -23.9, -46.3, DateTimeOffset.UtcNow);

    #endregion

    [Theory]
    [InlineData(-1, "distanceCm")]
    [InlineData(121, "distanceCm")]
    public async Task ProcessSensorData_ValidacaoDeDistancia_DeveLancarLogicException(
        double distance,
        string paramName
    )
    {
        // Arrange
        var payload = CreatePayload(distance);

        // Act & Assert
        await _service
            .Invoking(s => s.ProcessSensorDataAsync(payload))
            .Should()
            .ThrowAsync<LogicException>()
            .WithMessage($"*{paramName}*");
    }

    [Theory]
    [InlineData(90, 25, "Normal", false)]
    [InlineData(60, 50, "Alerta", true)]
    [InlineData(24, 80, "Crítico", true)]
    public async Task ProcessSensorData_StatusEBroadcast_DeveProcessarCorretamente(
        double distance,
        double obstruction,
        string expectedStatus,
        bool shouldBroadcast
    )
    {
        // Arrange
        var payload = CreatePayload(distance);

        // Act
        var result = await _service.ProcessSensorDataAsync(payload);

        // Assert
        result.Status.Should().Be(expectedStatus);
        result.NivelObstrucao.Should().Be(obstruction);

        _ingestionMock.Verify(
            i => i.SaveSensorDataAsync(It.IsAny<DrainStatusDTO>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        if (shouldBroadcast)
            _realtimeMock.Verify(rt => rt.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        else
            _realtimeMock.Verify(rt => rt.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task GetDrainStatus_DeveRetornarUltimoStatusDoRepositorio()
    {
        // Arrange
        var id = "DRN-02";
        var status = CreateStatus(id);
        _repositoryMock
            .Setup(r => r.GetLatestStatusAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);

        // Act
        var result = await _service.GetDrainStatusAsync(id);

        // Assert
        result.Should().BeEquivalentTo(status);
        _repositoryMock.Verify(
            r => r.GetLatestStatusAsync(id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetDrainStatus_NaoEncontrado_DeveLancarNotFoundException()
    {
        // Arrange
        const string id = "DRN-404";
        _repositoryMock
            .Setup(r => r.GetLatestStatusAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DrainStatusDTO?)null);

        // Act
        Func<Task> act = () => _service.GetDrainStatusAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ProcessSensorData_DeveGerarHashDeterministico()
    {
        // Arrange
        var dataFixa = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var payload = new SensorPayloadDTO("DRN-01", 90.0, -23.9, -46.3, dataFixa);

        // Act
        var result1 = await _service.ProcessSensorDataAsync(payload);
        var result2 = await _service.ProcessSensorDataAsync(payload);

        // Assert
        result1.DataHash.Should().NotBeNullOrWhiteSpace();
        result2.DataHash.Should().NotBeNullOrWhiteSpace();

        result1.DataHash.Should().Be(result2.DataHash);
        result1.DataHash.Length.Should().Be(64);
    }

    [Fact]
    public async Task ProcessSensorData_UltimaAtualizacaoNula_DeveNaoLancarErro_E_PreencherDataHashComFallback()
    {
        // Arrange
        var payload = new SensorPayloadDTO("DRN-NULL-TEST", 50.0); // UltimaAtualizacao is null

        // Act
        var result = await _service.ProcessSensorDataAsync(payload);

        // Assert
        result.DataHash.Should().NotBeNullOrWhiteSpace();
        result.DataHash.Length.Should().Be(64);
        result.UltimaAtualizacao.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }
}