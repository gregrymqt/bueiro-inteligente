// backend/Features/Uploads/Presentation/Controllers/UploadsController.cs
using backend.Extensions.App.Filters;
using backend.Features.Uploads.Application.DTOs;
using backend.Features.Uploads.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace backend.Features.Uploads.Presentation.Controllers;

public sealed class UploadsController(IUploadService uploadService) : ApiControllerBase
{
    private readonly IUploadService _uploadService =
        uploadService ?? throw new ArgumentNullException(nameof(uploadService));

    [HttpPost]
    [MaxFileSize]
    public async Task<ActionResult<UploadImagesDto>> UploadFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty or not provided.");
        }

        var result = await _uploadService.ProcessUploadAsync(file).ConfigureAwait(false);

        var desktopResponse = new UploadDto(
            result.Desktop.Id,
            result.Desktop.FileName,
            result.Desktop.ContentType,
            result.Desktop.Size,
            result.Desktop.Url, 
            result.Desktop.CreatedAt
        );

        var mobileResponse = new UploadDto(
            result.Mobile.Id,
            result.Mobile.FileName,
            result.Mobile.ContentType,
            result.Mobile.Size,
            result.Mobile.Url, 
            result.Mobile.CreatedAt
        );

        var response = new UploadImagesDto(desktopResponse, mobileResponse);

        return Created(result.Desktop.Url, response);
    }
}