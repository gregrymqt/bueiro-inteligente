using backend.Features.Uploads.Domain;
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