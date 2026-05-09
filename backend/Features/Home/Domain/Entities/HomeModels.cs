using backend.Features.Uploads.Domain.Entities;

namespace backend.Features.Home.Domain.Entities;

public sealed class CarouselModel(
    Guid id = default,
    string title = "",
    Guid uploadId = default,
    CarouselSection section = CarouselSection.hero,
    string? subtitle = null,
    string? actionUrl = null,
    int order = 0
)
{
    public Guid Id { get; set; } = id == Guid.Empty ? Guid.NewGuid() : id;

    public required string Title { get; set; } = title;

    public string? Subtitle { get; set; } = subtitle;

    public required Guid UploadId { get; set; } = uploadId;

    public UploadModel? Upload { get; set; }

    public string? ActionUrl { get; set; } = actionUrl;

    public int Order { get; set; } = order;

    public required CarouselSection Section { get; set; } = section;
}

/// <summary>
/// Combined Home content returned by the repository layer.
/// </summary>
public sealed record HomeContent(
    IReadOnlyList<CarouselModel> Carousels,
    IReadOnlyList<StatCardModel> Stats
);

public enum CarouselSection
{
    hero,
    alerts,
    stats,
}

public enum StatCardColor
{
    success,
    warning,
    danger,
}

public sealed class StatCardModel(
    Guid id = default,
    string title = "",
    string value = "",
    string description = "",
    string iconName = "",
    StatCardColor color = StatCardColor.success,
    int order = 0
)
{
    public Guid Id { get; set; } = id == Guid.Empty ? Guid.NewGuid() : id;

    public required string Title { get; set; } = title;

    public required string Value { get; set; } = value;

    public required string Description { get; set; } = description;

    public required string IconName { get; set; } = iconName;

    public required StatCardColor Color { get; set; } = color;

    public int Order { get; set; } = order;
}
