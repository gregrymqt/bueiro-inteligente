namespace backend.Tests.Features.Monitoring;

public sealed class MonitoringServiceTests
{
    private readonly Mock<IMonitoringRepository> _repositoryMock = new(MockBehavior.Strict);
    private readonly Mock<ICacheService> _cacheMock = new(MockBehavior.Strict);
    private readonly Mock<IRealtimeService> _realtimeMock = new(MockBehavior.Strict);
    private readonly MonitoringService _service;

    public MonitoringServiceTests()
    {
        _service = new MonitoringService(
            _repositoryMock.Object,
            _cacheMock.Object,
            _realtimeMock.Object,
            Mock.Of<ILogger<MonitoringService>>()
        );
    }

    /// <summary>
    /// Falha de hardware IoT: uma leitura negativa deve ser rejeitada antes de qualquer persistência.
    /// </summary>
    [Fact]
    public async Task ProcessSensorDataAsync_ComDistanciaNegativa_DeveLancarLogicException()
    {
        // Arrange
        SensorPayloadDTO payload = BuildPayload(-1);

        // Act
        Func<Task> act = () => _service.ProcessSensorDataAsync(payload);

        // Assert
        await act.Should().ThrowAsync<LogicException>()
            .WithMessage("*distanceCm*");

        _repositoryMock.VerifyNoOtherCalls();
        _cacheMock.VerifyNoOtherCalls();
        _realtimeMock.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Falha de hardware IoT: uma leitura acima do limite físico do sensor deve ser rejeitada.
    /// </summary>
    [Fact]
    public async Task ProcessSensorDataAsync_ComDistanciaAcimaDoMaximo_DeveLancarLogicException()
    {
        // Arrange
        SensorPayloadDTO payload = BuildPayload(121);

        // Act
        Func<Task> act = () => _service.ProcessSensorDataAsync(payload);

        // Assert
        await act.Should().ThrowAsync<LogicException>()
            .WithMessage("*distanceCm*");

        _repositoryMock.VerifyNoOtherCalls();
        _cacheMock.VerifyNoOtherCalls();
        _realtimeMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessSensorDataAsync_ComNivelNormal_DeveCalcularStatusNormalESalvar()
    {
        // Arrange
        SensorPayloadDTO payload = BuildPayload(90);

        _repositoryMock
            .Setup(repository => repository.SaveSensorDataAsync(
                It.Is<DrainStatusDTO>(status =>
                    status.IdBueiro == payload.IdBueiro
                    && status.DistanciaCm == 90
                    && status.NivelObstrucao == 25
                    && status.Status == "Normal"
                    && status.Latitude == payload.Latitude
                    && status.Longitude == payload.Longitude),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        DateTimeOffset before = DateTimeOffset.UtcNow;

        // Act
        DrainStatusDTO result = await _service.ProcessSensorDataAsync(payload);

        DateTimeOffset after = DateTimeOffset.UtcNow;

        // Assert
        result.IdBueiro.Should().Be(payload.IdBueiro);
        result.DistanciaCm.Should().Be(90);
        result.NivelObstrucao.Should().Be(25);
        result.Status.Should().Be("Normal");
        result.Latitude.Should().Be(payload.Latitude);
        result.Longitude.Should().Be(payload.Longitude);
        result.UltimaAtualizacao.Should().BeOnOrAfter(before);
        result.UltimaAtualizacao.Should().BeOnOrBefore(after);

        _repositoryMock.Verify(repository => repository.SaveSensorDataAsync(It.IsAny<DrainStatusDTO>(), It.IsAny<CancellationToken>()), Times.Once);
        _realtimeMock.Verify(realtime => realtime.BroadcastMonitoringData(It.IsAny<object>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
        _cacheMock.VerifyNoOtherCalls();
        _realtimeMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessSensorDataAsync_ComNivelAlerta_DeveDispararBroadcast()
    {
        // Arrange
        SensorPayloadDTO payload = BuildPayload(60);

        _repositoryMock
            .Setup(repository => repository.SaveSensorDataAsync(
                It.Is<DrainStatusDTO>(status =>
                    status.IdBueiro == payload.IdBueiro
                    && status.DistanciaCm == 60
                    && status.NivelObstrucao == 50
                    && status.Status == "Alerta"),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _realtimeMock
            .Setup(realtime => realtime.BroadcastMonitoringData(
                It.Is<DrainStatusDTO>(status =>
                    status.IdBueiro == payload.IdBueiro
                    && status.Status == "Alerta"
                    && status.DistanciaCm == 60
                    && status.NivelObstrucao == 50)))
            .Returns(Task.CompletedTask);

        // Act
        DrainStatusDTO result = await _service.ProcessSensorDataAsync(payload);

        // Assert
        result.Status.Should().Be("Alerta");
        result.NivelObstrucao.Should().Be(50);

        _repositoryMock.Verify(repository => repository.SaveSensorDataAsync(It.IsAny<DrainStatusDTO>(), It.IsAny<CancellationToken>()), Times.Once);
        _realtimeMock.Verify(realtime => realtime.BroadcastMonitoringData(It.IsAny<object>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _cacheMock.VerifyNoOtherCalls();
        _realtimeMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessSensorDataAsync_ComNivelCritico_DeveDispararBroadcast()
    {
        // Arrange
        SensorPayloadDTO payload = BuildPayload(24);

        _repositoryMock
            .Setup(repository => repository.SaveSensorDataAsync(
                It.Is<DrainStatusDTO>(status =>
                    status.IdBueiro == payload.IdBueiro
                    && status.DistanciaCm == 24
                    && status.NivelObstrucao == 80
                    && status.Status == "Crítico"),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _realtimeMock
            .Setup(realtime => realtime.BroadcastMonitoringData(
                It.Is<DrainStatusDTO>(status =>
                    status.IdBueiro == payload.IdBueiro
                    && status.Status == "Crítico"
                    && status.DistanciaCm == 24
                    && status.NivelObstrucao == 80)))
            .Returns(Task.CompletedTask);

        // Act
        DrainStatusDTO result = await _service.ProcessSensorDataAsync(payload);

        // Assert
        result.Status.Should().Be("Crítico");
        result.NivelObstrucao.Should().Be(80);

        _repositoryMock.Verify(repository => repository.SaveSensorDataAsync(It.IsAny<DrainStatusDTO>(), It.IsAny<CancellationToken>()), Times.Once);
        _realtimeMock.Verify(realtime => realtime.BroadcastMonitoringData(It.IsAny<object>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _cacheMock.VerifyNoOtherCalls();
        _realtimeMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDrainStatusAsync_QuandoExisteNoCache_NaoDeveChamarRepositorio()
    {
        // Arrange
        string drainIdentifier = "DRN-01";
        DrainStatusDTO cachedStatus = BuildStatus(
            drainIdentifier,
            45,
            62.5,
            "Alerta",
            -23.5505,
            -46.6333
        );

        _cacheMock
            .Setup(cache => cache.GetOrSetAsync(
                It.Is<string>(key => key == $"bueiro:{drainIdentifier}:status"),
                It.IsAny<Func<Task<DrainStatusDTO>>>(),
                It.Is<TimeSpan?>(expiry => expiry == TimeSpan.FromHours(1))))
            .ReturnsAsync(new CacheResponseDto<DrainStatusDTO>(cachedStatus, true));

        // Act
        DrainStatusDTO result = await _service.GetDrainStatusAsync(drainIdentifier);

        // Assert
        result.Should().BeEquivalentTo(cachedStatus);

        _cacheMock.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<DrainStatusDTO>>>(), It.IsAny<TimeSpan?>()), Times.Once);
        _repositoryMock.Verify(repository => repository.GetLatestStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
        _cacheMock.VerifyNoOtherCalls();
        _realtimeMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDrainStatusAsync_QuandoCacheMiss_DeveBuscarNoDBEAtualizarCache()
    {
        // Arrange
        string drainIdentifier = "DRN-02";
        DrainStatusDTO dbStatus = BuildStatus(
            drainIdentifier,
            30,
            75,
            "Alerta",
            -23.6101,
            -46.5678
        );

        _repositoryMock
            .Setup(repository => repository.GetLatestStatusAsync(drainIdentifier, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbStatus);

        _cacheMock
            .Setup(cache => cache.GetOrSetAsync(
                It.Is<string>(key => key == $"bueiro:{drainIdentifier}:status"),
                It.IsAny<Func<Task<DrainStatusDTO>>>(),
                It.Is<TimeSpan?>(expiry => expiry == TimeSpan.FromHours(1))))
            .Returns(async (string key, Func<Task<DrainStatusDTO>> fetchFunc, TimeSpan? expiry) =>
            {
                DrainStatusDTO fetched = await fetchFunc().ConfigureAwait(false);
                return new CacheResponseDto<DrainStatusDTO>(fetched, false);
            });

        // Act
        DrainStatusDTO result = await _service.GetDrainStatusAsync(drainIdentifier);

        // Assert
        result.Should().BeEquivalentTo(dbStatus);

        _cacheMock.Verify(cache => cache.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<DrainStatusDTO>>>(), It.IsAny<TimeSpan?>()), Times.Once);
        _repositoryMock.Verify(repository => repository.GetLatestStatusAsync(drainIdentifier, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
        _cacheMock.VerifyNoOtherCalls();
        _realtimeMock.VerifyNoOtherCalls();
    }

    private static SensorPayloadDTO BuildPayload(double distanceCm)
    {
        return new SensorPayloadDTO("DRN-01", distanceCm, -23.5505, -46.6333);
    }

    private static DrainStatusDTO BuildStatus(
        string drainIdentifier,
        double distanceCm,
        double obstructionLevel,
        string status,
        double? latitude,
        double? longitude
    )
    {
        return new DrainStatusDTO
        {
            IdBueiro = drainIdentifier,
            DistanciaCm = distanceCm,
            NivelObstrucao = obstructionLevel,
            Status = status,
            Latitude = latitude,
            Longitude = longitude,
            UltimaAtualizacao = new DateTimeOffset(2026, 4, 11, 10, 0, 0, TimeSpan.Zero),
        };
    }
}