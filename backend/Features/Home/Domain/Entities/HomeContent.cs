using backend.Features.Home.Domain.Entities;

namespace backend.Features.Home.Domain.Entities;

/// <summary>
/// Combined Home content returned by the repository layer.
/// </summary>
public sealed record HomeContent(
    IReadOnlyList<CarouselModel> Carousels,
    IReadOnlyList<StatCardModel> Stats
);