using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace backend.Core
{
    public sealed class AppSettings
    {
        public static AppSettings Current { get; } = Load();

        public AppSettings(
            string projectName,
            string version,
            string apiStr,
            string secretKey,
            string algorithm,
            int accessTokenExpireMinutes,
            string hardwareToken,
            string redisUrl,
            bool redisLocal,
            bool dbLocal,
            string databaseUrlCloud,
            string databaseUrlLocal,
            string databaseUrl,
            string migrationsUrl,
            string rowsApiKey,
            string rowsBaseUrl,
            string rowsSpreadsheetId,
            string rowsTableId,
            string googleClientId,
            string googleClientSecret,
            string[] allowedOrigins,
            bool dotnetSystemGlobalizationInvariant,
            string[]? emailUsersAdmin = null,
            string allowedHosts = "*",
            string appIdSecret = ""
        )
        {
            ProjectName = projectName;
            Version = version;
            ApiStr = apiStr;
            SecretKey = secretKey;
            Algorithm = algorithm;
            AccessTokenExpireMinutes = accessTokenExpireMinutes;
            HardwareToken = hardwareToken;
            RedisUrl = redisUrl;
            RedisLocal = redisLocal;
            DbLocal = dbLocal;
            DatabaseUrlCloud = databaseUrlCloud;
            DatabaseUrlLocal = databaseUrlLocal;
            DatabaseUrl = databaseUrl;
            MigrationsUrl = migrationsUrl;
            RowsApiKey = rowsApiKey;
            RowsBaseUrl = rowsBaseUrl;
            RowsSpreadsheetId = rowsSpreadsheetId;
            RowsTableId = rowsTableId;
            GoogleClientId = googleClientId;
            GoogleClientSecret = googleClientSecret;
            AllowedOrigins = allowedOrigins;
            AllowedHosts = allowedHosts;
            AppIdSecret = appIdSecret;
            DotNetSystemGlobalizationInvariant = dotnetSystemGlobalizationInvariant;
            EmailUsersAdmin = emailUsersAdmin ?? Array.Empty<string>();
        }

        public AppSettings(
            string projectName,
            string version,
            string apiStr,
            string secretKey,
            string algorithm,
            int accessTokenExpireMinutes,
            string hardwareToken,
            string redisUrl,
            bool redisLocal,
            bool dbLocal,
            string databaseUrlCloud,
            string databaseUrlLocal,
            string databaseUrl,
            string migrationsUrl,
            string rowsApiKey,
            string rowsBaseUrl,
            string rowsSpreadsheetId,
            string rowsTableId,
            string[] allowedOrigins,
            bool dotnetSystemGlobalizationInvariant
        )
            : this(
                projectName,
                version,
                apiStr,
                secretKey,
                algorithm,
                accessTokenExpireMinutes,
                hardwareToken,
                redisUrl,
                redisLocal,
                dbLocal,
                databaseUrlCloud,
                databaseUrlLocal,
                databaseUrl,
                migrationsUrl,
                rowsApiKey,
                rowsBaseUrl,
                rowsSpreadsheetId,
                rowsTableId,
                string.Empty,
                string.Empty,
                allowedOrigins,
                dotnetSystemGlobalizationInvariant,
                Array.Empty<string>()
            ) { }

        public string ProjectName { get; }

        public string Version { get; }

        public string ApiStr { get; }

        public string SecretKey { get; }

        public string Algorithm { get; }

        public int AccessTokenExpireMinutes { get; }

        public string HardwareToken { get; }

        public string RedisUrl { get; }

        public bool RedisLocal { get; }

        public bool DbLocal { get; }

        public string DatabaseUrlCloud { get; }

        public string DatabaseUrlLocal { get; }

        public string DatabaseUrl { get; }

        public string MigrationsUrl { get; }

        public string RowsApiKey { get; }

        public string RowsBaseUrl { get; }

        public string RowsSpreadsheetId { get; }

        public string RowsTableId { get; }

        public string GoogleClientId { get; }

        public string GoogleClientSecret { get; }

        public string[] AllowedOrigins { get; }

        public string AllowedHosts { get; }

        public string AppIdSecret { get; }

        public string[] EmailUsersAdmin { get; }

        public bool DotNetSystemGlobalizationInvariant { get; }

        public static AppSettings Reload()
        {
            return Load();
        }

        private static AppSettings Load()
        {
            LoadDotEnv();

            bool dbLocal = GetBool("DB_LOCAL", false);
            string databaseUrlCloud = GetString("DATABASE_URL_CLOUD");
            string databaseUrlLocal = GetString("DATABASE_URL_LOCAL");

            return new AppSettings(
                projectName: GetString("PROJECT_NAME", "Bueiro Inteligente"),
                version: GetString("VERSION", "1.0.0"),
                apiStr: GetString("API_STR", "/api/v1"),
                secretKey: GetString("SECRET_KEY"),
                algorithm: GetString("ALGORITHM", "HS256"),
                accessTokenExpireMinutes: GetInt("ACCESS_TOKEN_EXPIRE_MINUTES", 30),
                hardwareToken: GetString("HARDWARE_TOKEN"),
                redisUrl: GetString("REDIS_URL"),
                redisLocal: GetBool("REDIS_LOCAL", false),
                dbLocal: dbLocal,
                databaseUrlCloud: databaseUrlCloud,
                databaseUrlLocal: databaseUrlLocal,
                databaseUrl: dbLocal ? databaseUrlLocal : databaseUrlCloud,
                migrationsUrl: GetString("MIGRATIONS_URL"),
                rowsApiKey: GetString("ROWS_API_KEY"),
                rowsBaseUrl: GetString("ROWS_BASE_URL", "https://api.rows.com/v1"),
                rowsSpreadsheetId: GetString("ROWS_SPREADSHEET_ID"),
                rowsTableId: GetString("ROWS_TABLE_ID"),
                googleClientId: GetString("GOOGLE_CLIENT_ID"),
                googleClientSecret: GetString("GOOGLE_CLIENT_SECRET"),
                allowedOrigins: GetAllowedOrigins("ALLOWED_ORIGINS", ["*"]),
                allowedHosts: GetString("ALLOWED_HOSTS", GetString("AllowedHosts", "*")),
                appIdSecret: GetString("APP_ID_SECRET", GetString("AppIdSecret")),
                dotnetSystemGlobalizationInvariant: GetBool(
                    "DOTNET_SYSTEM_GLOBALIZATION_INVARIANT",
                    true
                ),
                emailUsersAdmin: GetDelimitedStrings("EMAIL_USERS_ADMIN")
            );
        }

        private static void LoadDotEnv()
        {
            string? envFilePath = FindDotEnvFile();

            if (envFilePath is null)
            {
                return;
            }

            foreach (string line in File.ReadAllLines(envFilePath))
            {
                string trimmedLine = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#'))
                {
                    continue;
                }

                if (trimmedLine.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
                {
                    trimmedLine = trimmedLine[7..].TrimStart();
                }

                int separatorIndex = trimmedLine.IndexOf('=');

                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = trimmedLine[..separatorIndex].Trim();
                string value = trimmedLine[(separatorIndex + 1)..].Trim();

                if (value.Length >= 2)
                {
                    bool isDoubleQuoted = value.StartsWith('"') && value.EndsWith('"');
                    bool isSingleQuoted = value.StartsWith('\'') && value.EndsWith('\'');

                    if (isDoubleQuoted || isSingleQuoted)
                    {
                        value = value[1..^1];
                    }
                }

                if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }

        private static string? FindDotEnvFile()
        {
            DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

            while (currentDirectory is not null)
            {
                string candidatePath = Path.Combine(currentDirectory.FullName, ".env");

                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }

                currentDirectory = currentDirectory.Parent;
            }

            return null;
        }

        private static string GetString(string key, string defaultValue = "")
        {
            return Environment.GetEnvironmentVariable(key) ?? defaultValue;
        }

        private static int GetInt(string key, int defaultValue)
        {
            string? rawValue = Environment.GetEnvironmentVariable(key);

            return int.TryParse(
                rawValue,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int parsedValue
            )
                ? parsedValue
                : defaultValue;
        }

        private static bool GetBool(string key, bool defaultValue)
        {
            string? rawValue = Environment.GetEnvironmentVariable(key);

            if (bool.TryParse(rawValue, out bool parsedValue))
            {
                return parsedValue;
            }

            if (
                string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rawValue, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rawValue, "True", StringComparison.OrdinalIgnoreCase)
            )
            {
                return true;
            }

            if (
                string.Equals(rawValue, "0", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rawValue, "no", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rawValue, "False", StringComparison.OrdinalIgnoreCase)
            )
            {
                return false;
            }

            return defaultValue;
        }

        private static string[] GetAllowedOrigins(string key, string[] defaultValue)
        {
            string? rawValue = Environment.GetEnvironmentVariable(key);

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return defaultValue;
            }

            string trimmedValue = rawValue.Trim();

            if (
                trimmedValue.StartsWith("[", StringComparison.Ordinal)
                && trimmedValue.EndsWith("]", StringComparison.Ordinal)
            )
            {
                try
                {
                    string[]? parsedValue = JsonSerializer.Deserialize<string[]>(trimmedValue);

                    if (parsedValue is not null && parsedValue.Length > 0)
                    {
                        return parsedValue;
                    }
                }
                catch (JsonException) { }
            }

            string[] splitOrigins = rawValue.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );

            return
            [
                .. splitOrigins
                    .Select(o => o.Replace("\"", "").Replace("'", "").Trim())
                    .Where(o => !string.IsNullOrEmpty(o)),
            ];
        }

        private static string[] GetDelimitedStrings(string key)
        {
            string? rawValue = Environment.GetEnvironmentVariable(key);

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return [];
            }

            string trimmedValue = rawValue.Trim();

            return trimmedValue.Split(
                [',', ';', '\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
        }
    }
}
