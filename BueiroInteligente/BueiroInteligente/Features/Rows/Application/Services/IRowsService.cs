using BueiroInteligente.Features.Rows.Application.DTOs;

namespace BueiroInteligente.Features.Rows.Application.Services;

/// <summary>
/// Defines the integration contract with the Rows API.
/// </summary>
public interface IRowsService
{
    /// <summary>Appends data rows to a Rows table.</summary>
    Task<bool> AppendDataAsync(
        string spreadsheetId,
        string tableId,
        RowsAppendRequest payload,
        CancellationToken cancellationToken = default
    );

    /// <summary>Creates a new table in a Rows page.</summary>
    Task<RowsCreateTableResponse> CreateTableAsync(
        string spreadsheetId,
        string pageId,
        RowsCreateTableRequest payload,
        CancellationToken cancellationToken = default
    );
}