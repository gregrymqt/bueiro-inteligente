using backend.Extensions.App;
using backend.Extensions.Auth;
using backend.Extensions.Realtime;
using backend.Extensions.Scheduler;
using backend.Extensions.Security;
using backend.Extensions.Services.MercadoPago;
using backend.Extensions.Uploads;
using backend.Features.Rows.Infrastructure.Extensions;
using backend.Infrastructure;
using backend.Infrastructure.Cache;
using backend.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace backend.Extensions;

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

        services.AddBueiroInteligenteAuth(configuration);
        services.AddBueiroInteligenteSecurity();

        services.AddBueiroInteligenteHome();
        services.AddBueiroInteligenteDrains();
        services.AddBueiroInteligenteMonitoring();
        services.AddBueiroInteligenteRealtime();
        services.AddBueiroInteligenteRows();
        services.AddBueiroInteligenteScheduler();
        services.AddBueiroInteligenteUploads();
        services.AddBueiroInteligenteMercadoPago();

        return services;
    }
}
