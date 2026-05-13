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
    public async Task<ActionResult<UploadDto>> UploadFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty or not provided.");
        }

        var result = await _uploadService.ProcessUploadAsync(file).ConfigureAwait(false);

        // Devolvemos diretamente o result.Url que já contém o caminho relativo correto (/uploads/...)
        // ou o URL público absoluto do Supabase Storage, integrando nativamente com o ImageResolver.ts
        var response = new UploadDto(
            result.Id,
            result.FileName,
            result.ContentType,
            result.Size,
            result.Url, 
            result.CreatedAt
        );

        return Created(result.Url, response);
    }
}