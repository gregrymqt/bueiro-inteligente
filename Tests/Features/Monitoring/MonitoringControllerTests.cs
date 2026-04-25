using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace backend.Tests.Features.Monitoring;

public sealed class MonitoringControllerTests
{
    private readonly Mock<IMonitoringService> _monitoringServiceMock = new();
    private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock = new();
    private readonly Mock<IAuthExtension> _authExtensionMock = new();
    private readonly MonitoringController _controller;

    public MonitoringControllerTests()
    {
        _controller = new MonitoringController(
            _monitoringServiceMock.Object,
            _backgroundJobClientMock.Object,
            _authExtensionMock.Object,
            Mock.Of<ILogger<MonitoringController>>()
        );
    }

    #region Helpers (O gabarito para telemetria)

    private void SetupContext(string? authHeader = null, string? queryToken = null)
    {
        var httpContext = new DefaultHttpContext();
        if (authHeader != null)
            httpContext.Request.Headers["Authorization"] = authHeader;
        if (queryToken != null)
            httpContext.Request.QueryString = new QueryString($"?token={queryToken}");

        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    private SensorPayloadDTO BuildPayload(string id = "DRN-01") => new(id, 70.0, -23.9, -46.3);

    #endregion

    [Fact]
    public async Task ReceiveSensorData_ComTokenInvalido_DeveRetornar401()
    {
        // Arrange
        var payload = BuildPayload();
        SetupContext(authHeader: "Bearer invalid", queryToken: "wrong");

        _authExtensionMock
            .Setup(a => a.VerifyHardwareToken(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new UnauthorizedAccessException("Hardware não autorizado"));

        // Act
        Func<Task> act = () => _controller.ReceiveSensorData(payload, default);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        _backgroundJobClientMock.Verify(
            j =>
                j.Create(
                    It.Is<Job>(job =>
                        job.Type == typeof(IMonitoringService)
                        && job.Method.Name == nameof(IMonitoringService.ProcessSensorDataAsync)
                        && job.Args.Count == 2
                    ),
                    It.IsAny<IState>()
                ),
            Times.Never
        );
        _monitoringServiceMock.Verify(
            s =>
                s.ProcessSensorDataAsync(
                    It.IsAny<SensorPayloadDTO>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Theory]
    [InlineData("NotFound")]
    [InlineData("LogicException")]
    public async Task GetStatus_CenariosDeErro_DevePropagarExcecao(string erroTipo)
    {
        // Arrange
        var drainId = "DRN-404";
        if (erroTipo == "NotFound")
            _monitoringServiceMock
                .Setup(s => s.GetDrainStatusAsync(drainId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("Bueiro", drainId));
        else
            _monitoringServiceMock
                .Setup(s => s.GetDrainStatusAsync(drainId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new LogicException("Erro de leitura"));

        // Act
        Func<Task> act = () => _controller.GetStatus(drainId, default);

        // Assert
        if (erroTipo == "NotFound")
            await act.Should().ThrowAsync<NotFoundException>();
        else
            await act.Should().ThrowAsync<LogicException>();
    }

    [Fact]
    public async Task ReceiveSensorData_Sucesso_DeveRetornarStatusProcessado()
    {
        // Arrange
        var payload = BuildPayload();
        SetupContext(queryToken: "valid-token");

        // Act
        var result = await _controller.ReceiveSensorData(payload, default);

        // Assert
        result.Should().BeOfType<AcceptedResult>();

        _backgroundJobClientMock.Verify(
            j =>
                j.Create(
                    It.Is<Job>(job =>
                        job.Type == typeof(IMonitoringService)
                        && job.Method.Name == nameof(IMonitoringService.ProcessSensorDataAsync)
                        && job.Args.Count == 2
                        && job.Args[0].GetType() == typeof(SensorPayloadDTO)
                        && (SensorPayloadDTO)job.Args[0] == payload
                    ),
                    It.Is<IState>(state => state.GetType() == typeof(EnqueuedState))
                ),
            Times.Once
        );

        _monitoringServiceMock.Verify(
            s =>
                s.ProcessSensorDataAsync(
                    It.IsAny<SensorPayloadDTO>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }
}
