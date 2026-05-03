namespace backend.Features.Monitoring.Domain.Configuration;

public sealed class BueiroConfiguration
{
    public string IdBueiro { get; set; } = string.Empty;
    public double MaxHeight { get; set; } = 120.0;
    public double CriticalThreshold { get; set; } = 80.0;
    public double AlertThreshold { get; set; } = 50.0;
}
