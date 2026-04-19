namespace backend.Core.Settings;

public sealed class GoogleSettings
{
    public const string SectionName = "Google";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string FrontendRedirectUrl { get; set; } = string.Empty;

    public string[] AllowedOrigins { get; set; } = ["*"];
}