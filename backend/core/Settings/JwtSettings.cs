namespace backend.Core.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;

    public string Algorithm { get; set; } = "HS256";

    public int AccessTokenExpireMinutes { get; set; } = 30;
}