namespace backend.Tests.Features.Monitoring;

public sealed class MonitoringControllerTests
{
    private readonly Mock<IMonitoringService> _monitoringServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IAuthExtension> _authExtensionMock = new(MockBehavior.Strict);
    private readonly Mock<IRateLimiter> _rateLimiterMock = new(MockBehavior.Strict);

    private readonly MonitoringController _controller;

    public MonitoringControllerTests()
    {
        _controller = new MonitoringController(
            _monitoringServiceMock.Object,
            _authExtensionMock.Object,
            _rateLimiterMock.Object,
            Mock.Of<ILogger<MonitoringController>>()
        );
    }

    [Fact]
    public async Task ReceiveSensorData_ComTokenDeHardwareInvalido_DeveRetornar401()
    {
        // Arrange
        SensorPayloadDTO payload = BuildPayload("DRN-10", 70);
        ConfigureHttpContext(
            path: "/monitoring/medicoes",
            authorizationHeader: "Bearer token-invalido",
            queryToken: "query-token"
        );

        _rateLimiterMock
            .Setup(rateLimiter => rateLimiter.EnforceAsync(It.IsAny<HttpContext>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _authExtensionMock
            .Setup(auth => auth.VerifyHardwareToken("Bearer token-invalido", "query-token"))
            .Throws(new UnauthorizedAccessException("Hardware inválido ou não autorizado."));

        // Act
        ActionResult<DrainStatusDTO> result = await _controller.ReceiveSensorData(payload, CancellationToken.None);

        // Assert
        ObjectResult unauthorizedResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        ProblemDetails problemDetails = unauthorizedResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        unauthorizedResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        problemDetails.Status.Should().Be(StatusCodes.Status401Unauthorized);
        problemDetails.Title.Should().Be("Unauthorized");
        problemDetails.Detail.Should().Be("Hardware inválido ou não autorizado.");

        _rateLimiterMock.Verify(rateLimiter => rateLimiter.EnforceAsync(It.IsAny<HttpContext>(), It.IsAny<CancellationToken>()), Times.Once);
        _authExtensionMock.Verify(auth => auth.VerifyHardwareToken("Bearer token-invalido", "query-token"), Times.Once);
        _monitoringServiceMock.Verify(service => service.ProcessSensorDataAsync(It.IsAny<SensorPayloadDTO>(), It.IsAny<CancellationToken>()), Times.Never);
        _rateLimiterMock.VerifyNoOtherCalls();
        _authExtensionMock.VerifyNoOtherCalls();
        _monitoringServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReceiveSensorData_ComRateLimitExcedido_DeveRetornar429()
    {
        // Arrange
        SensorPayloadDTO payload = BuildPayload("DRN-11", 60);
        ConfigureHttpContext(path: "/monitoring/medicoes");

        _rateLimiterMock
            .Setup(rateLimiter => rateLimiter.EnforceAsync(It.IsAny<HttpContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RateLimitExceededException(
                "Too Many Requests. Limite de requisições excedido.",
                TimeSpan.FromSeconds(15)
            ));

        // Act
        ActionResult<DrainStatusDTO> result = await _controller.ReceiveSensorData(payload, CancellationToken.None);

        // Assert
        ObjectResult objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        ProblemDetails problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        objectResult.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        problemDetails.Status.Should().Be(StatusCodes.Status429TooManyRequests);
        problemDetails.Title.Should().Be("Too many requests");
        problemDetails.Detail.Should().Contain("Limite de requisições");
        _controller.Response.Headers["Retry-After"].ToString().Should().Be("15");

        _rateLimiterMock.Verify(rateLimiter => rateLimiter.EnforceAsync(It.IsAny<HttpContext>(), It.IsAny<CancellationToken>()), Times.Once);
        _authExtensionMock.Verify(auth => auth.VerifyHardwareToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _monitoringServiceMock.Verify(service => service.ProcessSensorDataAsync(It.IsAny<SensorPayloadDTO>(), It.IsAny<CancellationToken>()), Times.Never);
        _rateLimiterMock.VerifyNoOtherCalls();
        _authExtensionMock.VerifyNoOtherCalls();
        _monitoringServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetStatus_QuandoBueiroNaoExiste_DeveRetornar404()
    {
        // Arrange
        string drainIdentifier = "DRN-404";
        ConfigureHttpContext(path: $"/monitoring/{drainIdentifier}/status");

        _rateLimiterMock
            .Setup(rateLimiter => rateLimiter.EnforceAsync(It.IsAny<HttpContext>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _monitoringServiceMock
            .Setup(service => service.GetDrainStatusAsync(drainIdentifier, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Bueiro", drainIdentifier));

        // Act
        ActionResult<DrainStatusDTO> result = await _controller.GetStatus(drainIdentifier, CancellationToken.None);

        // Assert
        ObjectResult objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        ProblemDetails problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Not found");
        problemDetails.Detail.Should().Contain(drainIdentifier);

        _rateLimiterMock.Verify(rateLimiter => rateLimiter.EnforceAsync(It.IsAny<HttpContext>(), It.IsAny<CancellationToken>()), Times.Once);
        _monitoringServiceMock.Verify(service => service.GetDrainStatusAsync(drainIdentifier, It.IsAny<CancellationToken>()), Times.Once);
        _authExtensionMock.Verify(auth => auth.VerifyHardwareToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _rateLimiterMock.VerifyNoOtherCalls();
        _authExtensionMock.VerifyNoOtherCalls();
        _monitoringServiceMock.VerifyNoOtherCalls();
    }

    private static SensorPayloadDTO BuildPayload(string drainIdentifier, double distanceCm)
    {
        return new SensorPayloadDTO(drainIdentifier, distanceCm, -23.5505, -46.6333);
    }

    private void ConfigureHttpContext(
        string path,
        string? authorizationHeader = null,
        string? queryToken = null
    )
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Path = path;

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            httpContext.Request.Headers["Authorization"] = authorizationHeader;
        }

        if (!string.IsNullOrWhiteSpace(queryToken))
        {
            httpContext.Request.QueryString = new QueryString($"?token={queryToken}");
        }

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
        };
    }
}