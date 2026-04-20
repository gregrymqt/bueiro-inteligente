using System.Net.Sockets;
using backend.Features.Auth.Domain;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace backend.Infrastructure;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

        services.AddDbContext<AppDbContext>(
            (sp, options) =>
            {
                var connectionString = GetDefaultConnectionString(configuration);

                options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure());
                options.EnableDetailedErrors();
            }
        );

        return services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    public static async Task InitializeBueiroInteligenteDatabaseAsync(
        this IServiceProvider sp,
        IConfiguration configuration,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(sp);
        ArgumentNullException.ThrowIfNull(configuration);

        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseBootstrap");
        var connectionString = GetMigrationConnectionString(configuration);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure())
            .Options;

        const int maxAttempts = 10;
        TimeSpan delay = TimeSpan.FromSeconds(2);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            await using var dbContext = new AppDbContext(options);

            try
            {
                await dbContext.Database.MigrateAsync(ct).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.LogCritical(
                    exception,
                    "Falha crítica ao conectar no banco via porta de Migration"
                );

                if (!IsTransientDatabaseException(exception) || attempt >= maxAttempts)
                {
                    throw;
                }

                await Task.Delay(delay, ct).ConfigureAwait(false);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 10));
                continue;
            }

            // Seed de Roles se necessário
            if (!await dbContext.Roles.AnyAsync(ct).ConfigureAwait(false))
            {
                dbContext.Roles.AddRange(
                    [
                        new() { Name = "User" },
                        new() { Name = "Admin" },
                        new() { Name = "Manager" },
                    ]
                );
                await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            return;
        }
    }

    private static bool IsTransientDatabaseException(Exception exception)
    {
        return exception is NpgsqlException
            || exception is SocketException
            || exception is TimeoutException;
    }

    private static string GetDefaultConnectionString(IConfiguration configuration) =>
        PostgreSqlConnectionStringFactory.Create(
            configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' não definida."
                )
        );

    private static string GetMigrationConnectionString(IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("MigrationsConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection strings 'MigrationsConnection' ou 'DefaultConnection' não definidas."
            );

        return PostgreSqlConnectionStringFactory.Create(connectionString);
    }

    internal static class PostgreSqlConnectionStringFactory
    {
        public static string Create(string databaseUrl)
        {
            if (string.IsNullOrWhiteSpace(databaseUrl))
                throw new InvalidOperationException("Connection string inválida ou ausente.");

            if (!databaseUrl.Contains("://", StringComparison.Ordinal))
                return databaseUrl;

            if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
                return databaseUrl;

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.IsDefaultPort ? 5432 : uri.Port,
                Database = uri.AbsolutePath.Trim('/'),
                Pooling = true,
                IncludeErrorDetail = true,
            };

            if (!string.IsNullOrWhiteSpace(uri.UserInfo))
            {
                var parts = uri.UserInfo.Split(':', 2);
                builder.Username = Uri.UnescapeDataString(parts[0]);
                if (parts.Length > 1)
                    builder.Password = Uri.UnescapeDataString(parts[1]);
            }

            return builder.ConnectionString;
        }
    }
}
