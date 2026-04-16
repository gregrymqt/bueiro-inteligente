using backend.Extensions.Auth;

namespace backend.Tests.Features.Auth;

public sealed class GoogleRedirectUrlResolverTests
{
    [Fact]
    public void ResolvePreferredFrontendRedirectUrl_DevePriorizarLocalQuandoDisponivel()
    {
        string result = GoogleRedirectUrlResolver.ResolvePreferredFrontendRedirectUrl(
            ["https://frontend.example.com", "http://localhost:5173", "https://abc.ngrok-free.app"]
        );

        result.Should().Be("http://localhost:5173");
    }

    [Fact]
    public void ResolvePreferredFrontendRedirectUrl_DevePriorizarTunelQuandoNaoHaLocal()
    {
        string result = GoogleRedirectUrlResolver.ResolvePreferredFrontendRedirectUrl(
            ["https://frontend.example.com", "https://abc.ngrok-free.app"]
        );

        result.Should().Be("https://abc.ngrok-free.app");
    }

    [Fact]
    public void ResolveFrontendRedirectUrl_DeveUsarOrigemSolicitadaQuandoPermitida()
    {
        string result = GoogleRedirectUrlResolver.ResolveFrontendRedirectUrl(
            "http://localhost:5173/path?x=1",
            ["http://localhost:5173", "https://frontend.example.com"],
            "https://frontend.example.com"
        );

        result.Should().Be("http://localhost:5173");
    }

    [Fact]
    public void ResolveFrontendRedirectUrl_DeveIgnorarOrigemNaoPermitida()
    {
        string result = GoogleRedirectUrlResolver.ResolveFrontendRedirectUrl(
            "https://malicious.example.com",
            ["http://localhost:5173", "https://frontend.example.com"],
            "https://frontend.example.com"
        );

        result.Should().Be("https://frontend.example.com");
    }
}
