namespace backend.Core.Settings;

public sealed class RedisSettings
{
    public const string SectionName = "Redis";

    public string Url { get; set; } = string.Empty;

    public bool Local { get; set; }
}