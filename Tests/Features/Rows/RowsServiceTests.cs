using System.Net;
using System.Text;
using backend.Core;
using backend.Features.Rows.Application.DTOs;
using backend.Features.Rows.Application.Services;
using Moq.Protected;

namespace backend.Tests.Features.Rows;

public sealed class RowsServiceTests
{
    private const string BaseAddress = "https://api.rows.com/v1/";
    private readonly Mock<IHttpClientFactory> _httpFactoryMock = new();
    private readonly RowsService _service;

    public RowsServiceTests()
    {
        _service = new RowsService(_httpFactoryMock.Object, Mock.Of<ILogger<RowsService>>());
    }

    #region Helpers (Blindagem contra código redundante)

    private void SetupRowsResponse(HttpStatusCode code, string content = "")
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage(code)
                {
                    Content = new StringContent(content, Encoding.UTF8, "application/json"),
                }
            );

        var client = new HttpClient(handlerMock.Object) { BaseAddress = new Uri(BaseAddress) };
        _httpFactoryMock.Setup(f => f.CreateClient("RowsApi")).Returns(client);
    }

    private RowsAppendRequest CreateRequest() =>
        new(
            [
                ["DRN-001", 12.5, 75d, "Alerta", true],
            ]
        );

    #endregion

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    public async Task AppendData_Sucesso_DeveRetornarTrue(HttpStatusCode code)
    {
        // Arrange
        SetupRowsResponse(code);

        // Act
        var result = await _service.AppendDataAsync("ss-123", "tb-456", CreateRequest(), default);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "401")]
    [InlineData(HttpStatusCode.InternalServerError, "500")]
    public async Task AppendData_ErroExterno_DeveLancarExternalApiException(
        HttpStatusCode code,
        string expectedMsg
    )
    {
        // Arrange
        SetupRowsResponse(code, "erro na api");

        // Act & Assert
        await _service
            .Invoking(s => s.AppendDataAsync("ss-123", "tb-456", CreateRequest(), default))
            .Should()
            .ThrowAsync<ExternalApiException>()
            .WithMessage($"*{expectedMsg}*");
    }
}
