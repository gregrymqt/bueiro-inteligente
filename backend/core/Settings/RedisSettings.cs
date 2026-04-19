namespace backend.Core.Settings;

public sealed class RedisSettings
{
    public const string SectionName = "Redis";

    public string RedisUrlLocal { get; set; } = string.Empty;
    public string RedisUrlCloud { get; set; } = string.Empty;

    public bool RedisLocal { get; set; }

    public string Url => RedisLocal ? RedisUrlLocal : RedisUrlCloud;
}
