namespace backend.Core.Settings;

public sealed class IotSettings
{
    public const string SectionName = "Iot";

    public string HardwareToken { get; set; } = string.Empty;
}