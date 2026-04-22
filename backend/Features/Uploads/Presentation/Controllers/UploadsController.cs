using backend.Extensions.App.Filters;
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
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty or not provided.");
        }

        var result = await _uploadService.ProcessUploadAsync(file).ConfigureAwait(false);
        return StatusCode(StatusCodes.Status201Created, result.Id);
    }
}
