namespace backend.Features.Uploads.Application.DTOs;

public sealed record UploadDto(
    Guid Id,
    string FileName,
    string ContentType,
    long Size,
    string AbsoluteUrl,
    DateTime CreatedAt
);
