using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BueiroInteligente.Features.Rows.Application.DTOs;

/// <summary>
/// Payload used by Rows append operations.
/// </summary>
public sealed record RowsAppendRequest(
    [property: JsonPropertyName("values"), Required]
    IReadOnlyList<IReadOnlyList<object?>> Values
);

/// <summary>
/// Payload used to create a new table in Rows.
/// </summary>
public sealed record RowsCreateTableRequest(
    [property: JsonPropertyName("name"), Required, StringLength(255)] string Name
);

/// <summary>
/// Response returned by Rows when a table is created.
/// </summary>
public sealed record RowsCreateTableResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt
);