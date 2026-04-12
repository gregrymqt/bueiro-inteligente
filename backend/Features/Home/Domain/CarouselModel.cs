namespace backend.Features.Home.Domain;

public sealed class CarouselModel(
    Guid id = default,
    string title = "",
    string imageUrl = "",
    CarouselSection section = CarouselSection.hero,
    string? subtitle = null,
    string? actionUrl = null,
    int order = 0
)
{
    public Guid Id { get; set; } = id == Guid.Empty ? Guid.NewGuid() : id;

    public required string Title { get; set; } = title;

    public string? Subtitle { get; set; } = subtitle;

    public required string ImageUrl { get; set; } = imageUrl;

    public string? ActionUrl { get; set; } = actionUrl;

    public int Order { get; set; } = order;

    public required CarouselSection Section { get; set; } = section;
}