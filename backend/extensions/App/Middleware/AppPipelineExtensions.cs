using backend.Extensions.App.Middleware;
using backend.Extensions.Auth;
using backend.Extensions.Security;
using backend.Infrastructure;
using backend.Infrastructure.Extensions;
using Serilog;
using MonitoringHub = backend.Features.Realtime.Presentation.MonitoringHub;

namespace backend.Extensions.App;

public static class AppPipelineExtensions
{
    public static async Task<WebApplication> UseBueiroInteligentePipeline(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var scopedServices = scope.ServiceProvider;

        // 1. Inicialização de Serviços (Onde costuma dar erro 500)
        await scopedServices.GetRequiredService<AuthExtension>().OpenAsync().ConfigureAwait(false);
        scopedServices.InitializeBueiroInteligenteSecurity();
        await scopedServices
            .InitializeBueiroInteligenteDatabaseAsync(app.Configuration)
            .ConfigureAwait(false);
        await scopedServices.InitializeBueiroInteligenteRedisAsync().ConfigureAwait(false);

        // 2. Middleware Pipeline
        app.UseBueiroInteligenteExceptionHandling(app.Environment);

        app.UseHttpsRedirection();
        app.UseRouting();

        // Log de requisições HTTP do Serilog
        app.UseSerilogRequestLogging();

        app.UseCors(AppServiceCollectionExtensions.RestrictedOriginsPolicyName);
        app.UseBueiroIntelligenteApp();

        app.UseAuthentication();
        app.UseAuthorization();

        // 3. Endpoints e Hubs
        app.MapHub<MonitoringHub>("/realtime/ws");
        app.MapControllers();
        app.MapGet("/health", () => Results.Ok()).AllowAnonymous();

        return app;
    }

    public static WebApplication UseBueiroIntelligenteApp(this WebApplication app)
    {
        app.UseMiddleware<AppIdMiddleware>();

        return app;
    }
}