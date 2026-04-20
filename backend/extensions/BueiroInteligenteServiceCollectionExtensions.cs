using backend.Extensions.App;
using backend.Extensions.Auth;
using backend.Extensions.Realtime;
using backend.Extensions.Security;
using backend.Features.Rows.Infrastructure.Extensions;
using backend.Infrastructure;
using backend.Infrastructure.Cache;
using backend.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions;

public static class BueiroInteligenteServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddBueiroInteligenteOptions(configuration);
        services.AddBueiroInteligenteApp(configuration);

        services.AddBueiroInteligenteDatabase();
        services.AddBueiroInteligenteRedis();
        services.AddBueiroInteligenteCache();

        services.AddBueiroInteligenteAuth(configuration);
        services.AddBueiroInteligenteSecurity();

        services.AddBueiroInteligenteHome();
        services.AddBueiroInteligenteMonitoring();
        services.AddBueiroInteligenteRealtime();
        services.AddBueiroInteligenteRows();
        services.AddBueiroInteligenteScheduler();

        return services;
    }
}