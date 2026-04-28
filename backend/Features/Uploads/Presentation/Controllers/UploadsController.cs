using backend.Extensions.App.Filters;
using backend.Features.Uploads.Application.DTOs;
using backend.Features.Uploads.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Uploads.Presentation.Controllers;

public sealed class UploadsController(IUploadService uploadService) : ApiControllerBase
{
    private readonly IUploadService _uploadService =
        uploadService ?? throw new ArgumentNullException(nameof(uploadService));

    [HttpPost]
    [MaxFileSize]
    public async Task<ActionResult<UploadDto>> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty or not provided.");
        }

        var result = await _uploadService.ProcessUploadAsync(file).ConfigureAwait(false);
        var absoluteUrl = BuildAbsoluteUploadUrl(HttpContext.Request, result.StoragePath);

        var response = new UploadDto(
            result.Id,
            result.FileName,
            result.ContentType,
            result.Size,
            absoluteUrl,
            result.CreatedAt
        );

        return Created(absoluteUrl, response);
    }

    private static string BuildAbsoluteUploadUrl(HttpRequest request, string storagePath)
    {
        var fileName = Path.GetFileName(storagePath);
        var relativePath = $"/uploads/{fileName}";

        return $"{request.Scheme}://{request.Host}{request.PathBase}{relativePath}";
    }
}
