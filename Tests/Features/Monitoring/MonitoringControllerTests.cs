namespace backend.Tests.Features.Monitoring;

public sealed class MonitoringControllerTests
{
    private readonly Mock<IMonitoringService> _monitoringServiceMock = new();
    private readonly Mock<IAuthExtension> _authExtensionMock = new();
    private readonly MonitoringController _controller;

    public MonitoringControllerTests()
    {
        _controller = new MonitoringController(
            _monitoringServiceMock.Object,
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
        var result = await _controller.ReceiveSensorData(payload, default);

        // Assert
        result
            .Result.Should()
            .BeAssignableTo<ObjectResult>()
            .Which.StatusCode.Should()
            .Be(StatusCodes.Status401Unauthorized);

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
    public async Task GetStatus_CenariosDeErro_DeveRetornarStatusCorreto(string erroTipo)
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
        var result = await _controller.GetStatus(drainId, default);

        // Assert
        var expectedStatus =
            erroTipo == "NotFound"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
        result
            .Result.Should()
            .BeAssignableTo<ObjectResult>()
            .Which.StatusCode.Should()
            .Be(expectedStatus);
    }

    [Fact]
    public async Task ReceiveSensorData_Sucesso_DeveRetornarStatusProcessado()
    {
        // Arrange
        var payload = BuildPayload();
        var expectedStatus = new DrainStatusDTO(
            payload.IdBueiro,
            payload.DistanciaCm,
            30.0,
            "NORMAL",
            payload.Latitude,
            payload.Longitude,
            DateTime.UtcNow
        );
        SetupContext(queryToken: "valid-token");

        _monitoringServiceMock
            .Setup(s => s.ProcessSensorDataAsync(payload, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.ReceiveSensorData(payload, default);

        // Assert
        result
            .Result.Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeEquivalentTo(expectedStatus);
    }
}
