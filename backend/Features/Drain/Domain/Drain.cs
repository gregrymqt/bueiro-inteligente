namespace backend.Features.Drain.Domain;

public sealed class Drain(
    Guid id = default,
    string name = "",
    string address = "",
    double latitude = 0d,
    double longitude = 0d,
    string hardwareId = "",
    bool isActive = true,
    DateTimeOffset CreatedAt = default
)
{
    public Guid Id { get; set; } = id == Guid.Empty ? Guid.NewGuid() : id;

    public required string Name { get; set; } = name;

    public required string Address { get; set; } = address;

    public double Latitude { get; set; } = latitude;

    public double Longitude { get; set; } = longitude;

    public bool IsActive { get; set; } = isActive;

    public required string HardwareId { get; set; } = hardwareId;

    public double MaxHeight { get; set; } = 120.0;

    public double CriticalThreshold { get; set; } = 80.0;

    public double AlertThreshold { get; set; } = 50.0;

    public DateTimeOffset CreatedAt { get; set; } =
        CreatedAt == default ? DateTimeOffset.UtcNow : CreatedAt;
}
