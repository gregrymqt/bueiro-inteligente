namespace backend.Core.Settings;

public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    public bool DbLocal { get; set; }

    public string DatabaseUrlCloud { get; set; } = string.Empty;

    public string DatabaseUrlLocal { get; set; } = string.Empty;

    public string MigrationsUrlCloud { get; set; } = string.Empty;
    public string MigrationsUrlLocal { get; set; } = string.Empty;

    public string MigrationsUrl => DbLocal ? MigrationsUrlLocal : MigrationsUrlCloud;

    public string DatabaseUrl => DbLocal ? DatabaseUrlLocal : DatabaseUrlCloud;
}
