using System.Globalization;
using System.Net.Sockets;
using backend.Features.Auth.Domain;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;

namespace backend.Infrastructure.Extensions; // Ajuste o namespace conforme seu padrão

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment) // Injetado para checar se é ambiente de Dev
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");
        var resolvedConnectionString = ResolveDevelopmentConnectionString(connectionString, environment);

        if (environment.IsDevelopment())
        {
            var diagnostic = new NpgsqlConnectionStringBuilder(resolvedConnectionString);
            Log.Information(
                "Database diagnostics: Host={Host}, Database={Database}, User={User}, RunningInContainer={RunningInContainer}",
                diagnostic.Host,
                diagnostic.Database,
                diagnostic.Username,
                IsRunningInsideContainer()
            );
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(resolvedConnectionString, npgsql =>
            {
                // Parâmetros explícitos são melhores para cloud (Render/Supabase)
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(15),
                    errorCodesToAdd: null);
            });

            // Ativa logs detalhados APENAS em desenvolvimento
            if (environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        // Caso seu AppDbContext implemente a interface IUnitOfWork diretamente
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static async Task InitializeBueiroInteligenteDatabaseAsync(
        this IServiceProvider sp,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(sp);

        // Cria um escopo para resolver o DbContext de forma segura
        using var scope = sp.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseBootstrap");

        const int maxAttempts = 5; // Reduzido, pois o Render/Supabase já tem boa disponibilidade
        TimeSpan delay = TimeSpan.FromSeconds(3);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                logger.LogInformation("Tentativa {Attempt}/{MaxAttempts}: Verificando conexão com o banco de dados...", attempt, maxAttempts);

                // Substitui o método manual inteiro de 'Probe' pelo recurso nativo do EF Core
                if (!await dbContext.Database.CanConnectAsync(ct).ConfigureAwait(false))
                {
                    throw new NpgsqlException("CanConnectAsync retornou falso. Banco de dados indisponível.");
                }

                logger.LogInformation("Conexão estabelecida. Aplicando migrações pendentes...");
                await dbContext.Database.MigrateAsync(ct).ConfigureAwait(false);

                await SeedRolesAsync(dbContext, ct);

                logger.LogInformation("Banco de dados inicializado com sucesso.");
                return; // Sai do loop em caso de sucesso
            }
            catch (Exception exception) when (IsTransientDatabaseException(exception) && attempt < maxAttempts)
            {
                logger.LogWarning(exception, "Falha transiente ao conectar ao banco. Tentando novamente em {Delay}s...", delay.TotalSeconds);
                await Task.Delay(delay, ct).ConfigureAwait(false);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 10)); // Backoff exponencial
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "Falha crítica e irrecuperável ao inicializar o banco de dados.");
                throw;
            }
        }
    }

    private static async Task SeedRolesAsync(AppDbContext dbContext, CancellationToken ct)
    {
        if (!await dbContext.Roles.AnyAsync(ct).ConfigureAwait(false))
        {
            dbContext.Roles.AddRange(
                new Role { Name = "User" },
                new Role { Name = "Admin" },
                new Role { Name = "Manager" }
            );
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
    }

    private static bool IsTransientDatabaseException(Exception exception)
    {
        if (exception is PostgresException postgresException)
        {
            return postgresException.SqlState is not "28P01" and not "28000";
        }

        return exception is NpgsqlException
            || exception is SocketException
            || exception is TimeoutException
            || exception.InnerException is SocketException
            || exception.InnerException is TimeoutException;
    }

    private static string ResolveDevelopmentConnectionString(
        string connectionString,
        IHostEnvironment environment
    )
    {
        if (!environment.IsDevelopment() || IsRunningInsideContainer())
        {
            return connectionString;
        }

        var builder = new NpgsqlConnectionStringBuilder(
            PostgreSqlConnectionStringFactory.Create(connectionString)
        );

        if (string.Equals(builder.Host, "db", StringComparison.OrdinalIgnoreCase))
        {
            builder.Host = "localhost";
        }

        return builder.ConnectionString;
    }

    private static bool IsRunningInsideContainer()
    {
        var value = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
    }

    public static class PostgreSqlConnectionStringFactory
    {
        public static string Create(string databaseUrl)
        {
            if (string.IsNullOrWhiteSpace(databaseUrl))
            {
                throw new InvalidOperationException("Connection string inválida ou ausente.");
            }

            if (!databaseUrl.Contains("://", StringComparison.Ordinal))
            {
                return databaseUrl;
            }

            if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
            {
                return databaseUrl;
            }

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.IsDefaultPort ? 5432 : uri.Port,
                Pooling = true,
                IncludeErrorDetail = true,
            };

            var databaseName = uri.AbsolutePath.Trim('/');
            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                builder.Database = databaseName;
            }

            if (!string.IsNullOrWhiteSpace(uri.UserInfo))
            {
                var parts = uri.UserInfo.Split(':', 2);
                builder.Username = Uri.UnescapeDataString(parts[0]);
                if (parts.Length > 1)
                {
                    builder.Password = Uri.UnescapeDataString(parts[1]);
                }
            }

            ApplyQueryParameters(builder, uri.Query);

            return builder.ConnectionString;
        }

        private static void ApplyQueryParameters(NpgsqlConnectionStringBuilder builder, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            foreach (
                var pair in query.TrimStart('?').Split(
                    '&',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                )
            )
            {
                var keyValue = pair.Split('=', 2);
                var key = Uri.UnescapeDataString(keyValue[0]).Trim();
                var value = keyValue.Length > 1
                    ? Uri.UnescapeDataString(keyValue[1]).Trim()
                    : string.Empty;

                switch (key.ToLowerInvariant())
                {
                    case "sslmode":
                        if (Enum.TryParse<SslMode>(value, ignoreCase: true, out var sslMode))
                        {
                            builder.SslMode = sslMode;
                        }

                        break;
                    case "pooling":
                        if (bool.TryParse(value, out var pooling))
                        {
                            builder.Pooling = pooling;
                        }

                        break;
                    case "timeout":
                    case "connect timeout":
                        if (
                            int.TryParse(
                                value,
                                NumberStyles.Integer,
                                CultureInfo.InvariantCulture,
                                out var timeout
                            )
                        )
                        {
                            builder.Timeout = timeout;
                        }

                        break;
                    case "commandtimeout":
                    case "command timeout":
                        if (
                            int.TryParse(
                                value,
                                NumberStyles.Integer,
                                CultureInfo.InvariantCulture,
                                out var commandTimeout
                            )
                        )
                        {
                            builder.CommandTimeout = commandTimeout;
                        }

                        break;
                    case "maxpoolsize":
                    case "maximum pool size":
                        if (
                            int.TryParse(
                                value,
                                NumberStyles.Integer,
                                CultureInfo.InvariantCulture,
                                out var maxPoolSize
                            )
                        )
                        {
                            builder.MaxPoolSize = maxPoolSize;
                        }

                        break;
                    case "minpoolsize":
                    case "minimum pool size":
                        if (
                            int.TryParse(
                                value,
                                NumberStyles.Integer,
                                CultureInfo.InvariantCulture,
                                out var minPoolSize
                            )
                        )
                        {
                            builder.MinPoolSize = minPoolSize;
                        }

                        break;
                    case "includeerrordetail":
                        if (bool.TryParse(value, out var includeErrorDetail))
                        {
                            builder.IncludeErrorDetail = includeErrorDetail;
                        }

                        break;
                    case "applicationname":
                    case "application name":
                        builder.ApplicationName = value;
                        break;
                }
            }
        }
    }
}