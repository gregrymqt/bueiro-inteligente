namespace BueiroInteligente.Features.Home.Domain;

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