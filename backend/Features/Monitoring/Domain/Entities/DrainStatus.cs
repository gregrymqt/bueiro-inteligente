namespace backend.Features.Monitoring.Domain.Entities;

public sealed class DrainStatus(
    Guid id = default,
    string drainIdentifier = "",
    double distanceCm = 0d,
    double obstructionLevel = 0d,
    string status = "",
    double? latitude = null,
    double? longitude = null,
    DateTimeOffset lastUpdate = default,
    bool syncedToRows = false,
    string dataHash = null!
)
{
    public Guid Id { get; set; } = id == Guid.Empty ? Guid.NewGuid() : id;

    public required string DrainIdentifier { get; set; } = drainIdentifier;

    public required double DistanceCm { get; set; } = distanceCm;

    public required double ObstructionLevel { get; set; } = obstructionLevel;

    public required string Status { get; set; } = status;

    public double? Latitude { get; set; } = latitude;

    public double? Longitude { get; set; } = longitude;

    public DateTimeOffset LastUpdate { get; set; } =
        lastUpdate == default ? DateTimeOffset.UtcNow : lastUpdate;

    public bool SyncedToRows { get; set; } = syncedToRows;

    public required string DataHash { get; set; } = dataHash;
}
