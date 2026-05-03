namespace backend.core.Settings;

public sealed class MercadoPagoSettings
{
    public const string SectionName = "MercadoPago";

    public string AccessToken { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string WebhookKey { get; set; } = string.Empty;
}
