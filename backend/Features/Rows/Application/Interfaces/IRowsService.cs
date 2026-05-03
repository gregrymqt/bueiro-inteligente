using backend.Features.Rows.Application.DTOs;

namespace backend.Features.Rows.Application.Interfaces;

public interface IRowsService
{
    Task<bool> AppendDataAsync(
        string spreadsheetId,
        string tableId,
        RowsAppendRequest payload,
        CancellationToken ct = default
    );
    Task<RowsCreateTableResponse> CreateTableAsync(
        string spreadsheetId,
        string pageId,
        RowsCreateTableRequest payload,
        CancellationToken ct = default
    );
}
