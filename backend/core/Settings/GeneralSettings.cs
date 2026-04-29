namespace backend.Core.Settings;

public sealed class GeneralSettings
{
    public const string SectionName = "General";

    public string ProjectName { get; set; } = "Bueiro Inteligente";

    public string Version { get; set; } = "1.0.0";

    public string ApiStr { get; set; } = "/api/v1";

    public string AllowedHosts { get; set; } = "*";

    public string[] AllowedOrigins { get; set; } = ["*"];

    public string[] EmailUsersAdmin { get; set; } = [];

    public string AppIdSecret { get; set; } = string.Empty;

    public bool DotNetSystemGlobalizationInvariant { get; set; } = true;

    /// <summary>
    /// Se true, executa EnsureDeletedAsync() antes de criar migrações.
    /// Útil para "resetar tudo" em ambientes de desenvolvimento.
    /// CUIDADO: Isso DELETA o banco de dados inteiro!
    /// </summary>
    public bool DatabaseResetNuclear { get; set; }

    /// <summary>
    /// Se true, tenta reparar problemas de migração automaticamente (ex: erro 42P07).
    /// Executará comandos SQL de 'stamp' na tabela __EFMigrationsHistory quando necessário.
    /// </summary>
    public bool DatabaseAutoRepairHistory { get; set; }
}
