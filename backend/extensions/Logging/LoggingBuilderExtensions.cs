using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace backend.Extensions.App.Logging;

public static class LoggingBuilderExtensions
{
    public static void AddBueiroInteligenteLogging(
        this ConfigureHostBuilder host,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() 
            // Silencia o ruído do Microsoft e do Quartz para você focar no seu código
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Quartz", LogEventLevel.Information) 
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.File(
                path: "log/log-.txt",
                rollingInterval: RollingInterval.Day,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();

        host.UseSerilog();
    }
}