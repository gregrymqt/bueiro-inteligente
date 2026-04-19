namespace backend.Core.Settings;

public sealed class GoogleSettings
{
    public const string SectionName = "Google";

    public string GoogleClientId { get; set; } = string.Empty;

    public string GoogleClientSecret { get; set; } = string.Empty;

    public string GoogleFrontendRedirectUrl { get; set; } = string.Empty;

    public string[] AllowedOrigins { get; set; } = ["*"];
}
