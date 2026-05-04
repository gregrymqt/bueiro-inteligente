using backend.extensions.Services.App;
using backend.extensions.Services.Auth;
using backend.extensions.Services.HangFire;
using backend.Extensions.Services.MercadoPago;
using backend.extensions.Services.Realtime;
using backend.extensions.Services.Rows;
using backend.extensions.Services.Security;
using backend.Infrastructure.Cache;
using backend.Infrastructure.Extensions;

namespace backend.extensions.Services;

public static class BueiroInteligenteServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        services.AddBueiroInteligenteOptions(configuration);
        services.AddBueiroInteligenteApp(configuration);

        services.AddBueiroInteligenteDatabase(configuration, environment);
        services.AddBueiroInteligenteRedis(configuration);
        services.AddBueiroInteligenteHangfire(configuration);
        services.AddBueiroInteligenteCache();

        services.AddBueiroInteligenteDependencyScanning();

        services.AddBueiroInteligenteAuth(configuration);
        services.AddBueiroInteligenteSecurity();

        services.AddBueiroInteligenteRealtime();
        services.AddBueiroInteligenteRows();
        services.AddBueiroInteligenteScheduler();
        services.AddBueiroInteligenteMercadoPago(configuration);

        return services;
    }
}
