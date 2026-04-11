namespace BueiroInteligente.Features.Drain.Domain;

public sealed class Drain(
    Guid id = default,
    string name = "",
    string address = "",
    double latitude = 0d,
    double longitude = 0d,
    string hardwareId = "",
    bool isActive = true,
    DateTimeOffset? createdAt = null
)
{
    public Guid Id { get; set; } = id == Guid.Empty ? Guid.NewGuid() : id;

    public required string Name { get; set; } = name;

    public required string Address { get; set; } = address;

    public double Latitude { get; set; } = latitude;

    public double Longitude { get; set; } = longitude;

    public bool IsActive { get; set; } = isActive;

    public required string HardwareId { get; set; } = hardwareId;

    public DateTimeOffset CreatedAt { get; set; } = createdAt ?? DateTimeOffset.UtcNow;
}
