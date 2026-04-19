namespace backend.Core.Settings;

public sealed class RowsSettings
{
    public const string SectionName = "Rows";

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.rows.com/v1";

    public string SpreadsheetId { get; set; } = string.Empty;

    public string TableId { get; set; } = string.Empty;
}