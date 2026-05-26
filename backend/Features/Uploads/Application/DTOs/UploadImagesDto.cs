namespace backend.Features.Uploads.Application.DTOs;

public sealed record UploadImagesDto(
    UploadDto Desktop,
    UploadDto Mobile
);
