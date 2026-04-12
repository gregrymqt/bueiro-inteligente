using backend.Core;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace backend.Infrastructure;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteDatabase(this IServiceCollection services)
    {
        services.AddSingleton(AppSettings.Current);

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

        services.AddDbContext<AppDbContext>(
            (serviceProvider, options) =>
            {
                string databaseUrl = serviceProvider.GetRequiredService<AppSettings>().DatabaseUrl;
                string resolvedConnectionString = PostgreSqlConnectionStringFactory.Create(
                    databaseUrl
                );

                options.UseNpgsql(
                    resolvedConnectionString,
                    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()
                );
                options.EnableDetailedErrors();
            }
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static async Task InitializeBueiroInteligenteDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }

    internal static class PostgreSqlConnectionStringFactory
    {
        public static string Create(string databaseUrl)
        {
            if (string.IsNullOrWhiteSpace(databaseUrl))
                throw new InvalidOperationException("DATABASE_URL não está definida.");

            string normalizedUrl = databaseUrl.Trim();

            if (!normalizedUrl.Contains("://", StringComparison.Ordinal))
                return normalizedUrl;

            if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out Uri? uri))
                throw new InvalidOperationException("DATABASE_URL possui um formato inválido.");

            if (!uri.Scheme.StartsWith("postgres", StringComparison.OrdinalIgnoreCase))
            {
                return normalizedUrl;
            }

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.IsDefaultPort ? 5432 : uri.Port,
                Pooling = true,
                IncludeErrorDetail = true,
            };

            string databaseName = uri.AbsolutePath.Trim('/');

            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                builder.Database = databaseName;
            }

            if (!string.IsNullOrWhiteSpace(uri.UserInfo))
            {
                string[] credentials = uri.UserInfo.Split(':', 2);
                builder.Username = Uri.UnescapeDataString(credentials[0]);

                if (credentials.Length > 1)
                {
                    builder.Password = Uri.UnescapeDataString(credentials[1]);
                }
            }

            foreach (KeyValuePair<string, string> queryParameter in ParseQueryString(uri.Query))
            {
                var key = queryParameter.Key.ToLower();
                var value = queryParameter.Value;

                if (key == "sslmode" && Enum.TryParse(value, true, out SslMode sslMode))
                {
                    builder.SslMode = sslMode;
                }
                // Adicione este suporte para o Supabase
                else if (key == "trustservercertificate" || key == "trust server certificate")
                {
                    if (value.Equals("require", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.SslMode = SslMode.Require;
                    }
                }
            }

            return builder.ConnectionString;
        }

        private static IReadOnlyDictionary<string, string> ParseQueryString(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (
                string segment in query
                    .TrimStart('?')
                    .Split('&', StringSplitOptions.RemoveEmptyEntries)
            )
            {
                string[] parts = segment.Split('=', 2);
                string key = Uri.UnescapeDataString(parts[0]);
                string value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;

                if (!string.IsNullOrWhiteSpace(key))
                {
                    values[key] = value;
                }
            }

            return values;
        }
    }
}
