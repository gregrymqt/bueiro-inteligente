namespace backend.Core.Settings;

public sealed class RedisSettings
{
    public const string SectionName = "Redis";

    public string UrlLocal { get; set; } = string.Empty;
    public string UrlCloud { get; set; } = string.Empty;

    public bool Local { get; set; }

    public string Url => Local ? UrlLocal : UrlCloud;
}
