using System.Net.Sockets;
using backend.Core.Settings;
using backend.Features.Auth.Domain;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace backend.Infrastructure;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteDatabase(this IServiceCollection services)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

        services.AddDbContext<AppDbContext>(
            (sp, options) =>
            {
                var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                var connectionString = PostgreSqlConnectionStringFactory.Create(
                    settings.DatabaseUrl
                );

                options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure());
                options.EnableDetailedErrors();
            }
        );

        return services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    public static async Task InitializeBueiroInteligenteDatabaseAsync(
        this IServiceProvider sp,
        CancellationToken ct = default
    )
    {
        using var scope = sp.CreateScope();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

        // Porta 5432 explícita para migrações em nuvem
        var connectionString = PostgreSqlConnectionStringFactory.Create(
            settings.DbLocal ? settings.DatabaseUrl : settings.MigrationsUrl
        );

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure())
            .Options;

        const int maxAttempts = 10;
        TimeSpan delay = TimeSpan.FromSeconds(2);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await using var dbContext = new AppDbContext(options);

                await dbContext.Database.MigrateAsync(ct).ConfigureAwait(false);

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
            catch (Exception exception) when (IsTransientDatabaseException(exception))
            {
                if (attempt >= maxAttempts)
                {
                    throw;
                }

                await Task.Delay(delay, ct).ConfigureAwait(false);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 10));
            }
        }
    }

    private static bool IsTransientDatabaseException(Exception exception)
    {
        return exception is NpgsqlException
            || exception is SocketException
            || exception is TimeoutException
            || exception is InvalidOperationException;
    }

    internal static class PostgreSqlConnectionStringFactory
    {
        public static string Create(string databaseUrl)
        {
            if (string.IsNullOrWhiteSpace(databaseUrl))
                throw new InvalidOperationException("DATABASE_URL não definida.");

            if (
                !databaseUrl.Contains("://")
                || !Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri)
            )
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
