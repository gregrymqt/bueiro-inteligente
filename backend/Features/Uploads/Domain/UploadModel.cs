using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Features.Uploads.Domain;

public class UploadModel
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string Checksum { get; set; } = string.Empty;
    [NotMapped]
    public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
