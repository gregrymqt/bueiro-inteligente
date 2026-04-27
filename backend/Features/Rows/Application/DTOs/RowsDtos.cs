using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Features.Rows.Application.DTOs;

/// <summary> Payload para operações de append no Rows. </summary>
public sealed record RowsAppendRequest(
    [property: JsonPropertyName("values"), Required] IReadOnlyList<IReadOnlyList<object?>> Values
);

/// <summary> Payload para criação de tabelas. </summary>
public sealed record RowsCreateTableRequest([Required, StringLength(255)] string Name);

/// <summary> Resposta de criação de tabela. </summary>
public sealed record RowsCreateTableResponse(
    string Id,
    string Name,
    string Slug,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt
);
