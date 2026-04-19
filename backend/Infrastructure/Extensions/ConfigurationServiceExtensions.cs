using System.Collections;
using System.Globalization;
using backend.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.Extensions;

public static class ConfigurationServiceExtensions
{
    /// <summary>
    /// Mapeia as variáveis do .env e do sistema para o IConfiguration,
    /// normalizando as chaves para o formato de Seções do .NET.
    /// </summary>
    public static IConfigurationBuilder AddBueiroInteligenteDotEnvMappings(
        this IConfigurationBuilder configuration,
        string? startPath = null
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var rawEnv = LoadAllEnvironmentValues(startPath);
        var mappedValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        // Mapeamento Centralizado: Adicione novas seções aqui
        MapSection(
            mappedValues,
            rawEnv,
            GeneralSettings.SectionName,
            [
                "PROJECT_NAME",
                "VERSION",
                "API_STR",
                "APP_ID_SECRET",
                "DOTNET_SYSTEM_GLOBALIZATION_INVARIANT",
            ]
        );
        MapArray(
            mappedValues,
            rawEnv,
            GeneralSettings.SectionName,
            nameof(GeneralSettings.AllowedHosts),
            "ALLOWED_HOSTS"
        );
        MapArray(
            mappedValues,
            rawEnv,
            GeneralSettings.SectionName,
            nameof(GeneralSettings.AllowedOrigins),
            "ALLOWED_ORIGINS"
        );
        MapArray(
            mappedValues,
            rawEnv,
            GeneralSettings.SectionName,
            nameof(GeneralSettings.EmailUsersAdmin),
            "EMAIL_USERS_ADMIN"
        );
        MapSection(
            mappedValues,
            rawEnv,
            DatabaseSettings.SectionName,
            [
                "DB_LOCAL",
                "DATABASE_URL_CLOUD",
                "DATABASE_URL_LOCAL",
                "MIGRATIONS_URL_CLOUD",
                "MIGRATIONS_URL_LOCAL",
            ]
        );
        MapSection(
            mappedValues,
            rawEnv,
            JwtSettings.SectionName,
            ["SECRET_KEY", "ALGORITHM", "ACCESS_TOKEN_EXPIRE_MINUTES"]
        );
        MapSection(
            mappedValues,
            rawEnv,
            GoogleSettings.SectionName,
            ["GOOGLE_CLIENT_ID", "GOOGLE_CLIENT_SECRET", "GOOGLE_FRONTEND_REDIRECT_URL"]
        );
        MapSection(
            mappedValues,
            rawEnv,
            RedisSettings.SectionName,
            ["REDIS_URL_LOCAL", "REDIS_URL_CLOUD", "REDIS_LOCAL"]
        );
        MapSection(
            mappedValues,
            rawEnv,
            RowsSettings.SectionName,
            ["ROWS_API_KEY", "ROWS_BASE_URL", "ROWS_SPREADSHEET_ID", "ROWS_TABLE_ID"]
        );

        // Fallbacks para compatibilidade com Middlewares que lêem a raiz
        mappedValues["AllowedHosts"] = Resolve(rawEnv, "ALLOWED_HOSTS");
        mappedValues["AppIdSecret"] = Resolve(rawEnv, "APP_ID_SECRET");

        configuration.AddInMemoryCollection(mappedValues);
        return configuration;
    }

    public static IServiceCollection AddBueiroInteligenteOptions(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Configure<GeneralSettings>(config.GetSection(GeneralSettings.SectionName));
        services.Configure<DatabaseSettings>(config.GetSection(DatabaseSettings.SectionName));
        services.Configure<JwtSettings>(config.GetSection(JwtSettings.SectionName));
        services.Configure<GoogleSettings>(config.GetSection(GoogleSettings.SectionName));
        services.Configure<IotSettings>(config.GetSection(IotSettings.SectionName));
        services.Configure<RedisSettings>(config.GetSection(RedisSettings.SectionName));
        services.Configure<RowsSettings>(config.GetSection(RowsSettings.SectionName));

        return services;
    }

    #region Helpers de Mapeamento

    private static void MapSection(
        Dictionary<string, string?> target,
        IReadOnlyDictionary<string, string> source,
        string section,
        string[] keys
    )
    {
        foreach (var key in keys)
        {
            // Converte AP_ID_SECRET em AppIdSecret para bater com a propriedade da classe
            var propertyName = ToPascalCase(key);
            target[$"{section}:{propertyName}"] = Resolve(source, key);
        }
    }

    private static void MapArray(
        Dictionary<string, string?> target,
        IReadOnlyDictionary<string, string> source,
        string section,
        string property,
        string envKey
    )
    {
        var rawValue = Resolve(source, envKey);
        if (string.IsNullOrWhiteSpace(rawValue))
            return;

        // Suporte a JSON ou CSV (como no seu AppSettings.cs original)
        string[] values = rawValue.StartsWith('[')
            ? System.Text.Json.JsonSerializer.Deserialize<string[]>(rawValue) ?? []
            : rawValue.Split(
                ',',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
            );

        for (int i = 0; i < values.Length; i++)
            target[$"{section}:{property}:{i}"] = Sanitize(values[i]);
    }

    private static string? Resolve(IReadOnlyDictionary<string, string> source, string key) =>
        source.TryGetValue(key.Replace("_", ""), out var value) ? Sanitize(value) : null;

    private static string? Sanitize(string? value) => value?.Trim().Trim('"', '\'');

    private static string ToPascalCase(string key) =>
        string.Concat(
            key.Split('_')
                .Select(s => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLower()))
        );

    #endregion

    #region Carregamento de Arquivo

    private static Dictionary<string, string> LoadAllEnvironmentValues(string? startPath)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // 1. Carrega do .env (se existir)
        string? path = FindDotEnv(startPath ?? AppContext.BaseDirectory);
        if (path != null)
        {
            foreach (var line in File.ReadAllLines(path))
            {
                var parts = line.Split('=', 2);
                if (parts.Length != 2 || line.TrimStart().StartsWith('#'))
                    continue;
                var key = parts[0].Replace("export ", "").Trim();
                dict[key.Replace("_", "")] = parts[1].Trim();
            }
        }

        // 2. Sobrescreve com Variáveis de Sistema (Docker/Render tem prioridade)
        foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
            dict[env.Key.ToString()!.Replace("_", "")] = env.Value?.ToString() ?? "";

        return dict;
    }

    private static string? FindDotEnv(string path)
    {
        var dir = new DirectoryInfo(path);
        while (dir != null)
        {
            var file = Path.Combine(dir.FullName, ".env");
            if (File.Exists(file))
                return file;
            dir = dir.Parent;
        }
        return null;
    }

    #endregion
}
