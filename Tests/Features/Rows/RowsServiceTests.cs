using System.Net;
using System.Net.Http;
using System.Text;
using backend.Core;
using backend.Features.Rows.Application.DTOs;
using backend.Features.Rows.Application.Services;
using Microsoft.Extensions.Logging;
using Moq.Protected;

namespace backend.Tests.Features.Rows;

public sealed class RowsServiceTests
{
    private const string RowsClientName = "RowsApi";
    private const string BaseAddress = "https://api.rows.com/v1/";

    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new(MockBehavior.Strict);
    private readonly RowsService _service;

    public RowsServiceTests()
    {
        _service = new RowsService(_httpClientFactoryMock.Object, Mock.Of<ILogger<RowsService>>());
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    public async Task AppendDataAsync_ComSucesso_DeveRetornarTrue(HttpStatusCode statusCode)
    {
        // Arrange
        RowsAppendRequest payload = CreateAppendRequest();
        HttpRequestMessage? capturedRequest = null;
        string capturedRequestBody = string.Empty;

        Mock<HttpMessageHandler> handlerMock = CreateRowsHandler(
            statusCode,
            responseBody: string.Empty,
            reasonPhrase: statusCode == HttpStatusCode.Created ? "Created" : "OK",
            capturedRequest: (request, body) =>
            {
                capturedRequest = request;
                capturedRequestBody = body;
            }
        );

        SetupHttpClientFactory(handlerMock);

        // Act
        bool result = await _service.AppendDataAsync(
            "spreadsheet-123",
            "table-456",
            payload,
            CancellationToken.None
        );

        // Assert
        result.Should().BeTrue();
        _httpClientFactoryMock.Verify(factory => factory.CreateClient(RowsClientName), Times.Once);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.RequestUri!.AbsoluteUri.Should().Be(
            $"{BaseAddress}spreadsheets/spreadsheet-123/tables/table-456/values:append"
        );

        HttpContent requestContent = capturedRequest.Content ?? throw new InvalidOperationException("Request content was not captured.");

        capturedRequestBody.Should().Be("""{"values":[["DRN-001",12.5,75,"Alerta",true]]}""");
        requestContent.Headers.ContentType!.MediaType.Should().Be("application/json");

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Post
                && request.RequestUri != null
                && request.RequestUri.AbsoluteUri == $"{BaseAddress}spreadsheets/spreadsheet-123/tables/table-456/values:append"),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task AppendDataAsync_ComErroDeAutorizacao_DeveLancarExternalApiException()
    {
        // Arrange
        RowsAppendRequest payload = CreateAppendRequest();
        HttpRequestMessage? capturedRequest = null;
        string capturedRequestBody = string.Empty;

        Mock<HttpMessageHandler> handlerMock = CreateRowsHandler(
            HttpStatusCode.Unauthorized,
            responseBody: "autorizacao negada",
            reasonPhrase: "Unauthorized",
            capturedRequest: (request, body) =>
            {
                capturedRequest = request;
                capturedRequestBody = body;
            }
        );

        SetupHttpClientFactory(handlerMock);

        // Act
        Func<Task> act = () => _service.AppendDataAsync(
            "spreadsheet-123",
            "table-456",
            payload,
            CancellationToken.None
        );

        // Assert
        ExternalApiException exception = (await act.Should().ThrowAsync<ExternalApiException>()).Which;
        exception.ApiName.Should().Be("Rows");
        exception.Message.Should().Contain("401");
        exception.Message.Should().Contain("Unauthorized");
        exception.Message.Should().Contain("append_data");
        exception.Message.Should().Contain("autorizacao negada");

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequestBody.Should().NotBeNullOrWhiteSpace();

        _httpClientFactoryMock.Verify(factory => factory.CreateClient(RowsClientName), Times.Once);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Post
                && request.RequestUri != null
                && request.RequestUri.AbsoluteUri == $"{BaseAddress}spreadsheets/spreadsheet-123/tables/table-456/values:append"),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task AppendDataAsync_ComErroNoServidor_DeveLancarExternalApiException()
    {
        // Arrange
        RowsAppendRequest payload = CreateAppendRequest();
        HttpRequestMessage? capturedRequest = null;
        string capturedRequestBody = string.Empty;

        Mock<HttpMessageHandler> handlerMock = CreateRowsHandler(
            HttpStatusCode.InternalServerError,
            responseBody: "falha interna",
            reasonPhrase: "Internal Server Error",
            capturedRequest: (request, body) =>
            {
                capturedRequest = request;
                capturedRequestBody = body;
            }
        );

        SetupHttpClientFactory(handlerMock);

        // Act
        Func<Task> act = () => _service.AppendDataAsync(
            "spreadsheet-123",
            "table-456",
            payload,
            CancellationToken.None
        );

        // Assert
        ExternalApiException exception = (await act.Should().ThrowAsync<ExternalApiException>()).Which;
        exception.ApiName.Should().Be("Rows");
        exception.Message.Should().Contain("500");
        exception.Message.Should().Contain("Internal Server Error");
        exception.Message.Should().Contain("append_data");
        exception.Message.Should().Contain("falha interna");

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequestBody.Should().NotBeNullOrWhiteSpace();

        _httpClientFactoryMock.Verify(factory => factory.CreateClient(RowsClientName), Times.Once);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Post
                && request.RequestUri != null
                && request.RequestUri.AbsoluteUri == $"{BaseAddress}spreadsheets/spreadsheet-123/tables/table-456/values:append"),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    private static RowsAppendRequest CreateAppendRequest()
    {
        return new RowsAppendRequest(
            [
                ["DRN-001", 12.5, 75d, "Alerta", true]
            ]
        );
    }

    private void SetupHttpClientFactory(Mock<HttpMessageHandler> handlerMock)
    {
        HttpClient client = new(handlerMock.Object)
        {
            BaseAddress = new Uri(BaseAddress),
        };

        _httpClientFactoryMock
            .Setup(factory => factory.CreateClient(RowsClientName))
            .Returns(client);
    }

    private static Mock<HttpMessageHandler> CreateRowsHandler(
        HttpStatusCode statusCode,
        string responseBody,
        string reasonPhrase,
        Action<HttpRequestMessage, string> capturedRequest
    )
    {
        Mock<HttpMessageHandler> handlerMock = new(MockBehavior.Strict);

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Post
                    && request.RequestUri != null
                    && request.RequestUri.AbsoluteUri == $"{BaseAddress}spreadsheets/spreadsheet-123/tables/table-456/values:append"),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                string requestBody = request.Content is null
                    ? string.Empty
                    : request.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                capturedRequest(request, requestBody);
            })
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                ReasonPhrase = reasonPhrase,
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json"),
            });

        return handlerMock;
    }
}