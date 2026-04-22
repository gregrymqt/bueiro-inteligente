using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using backend.Features.Uploads.Domain;
using backend.Features.Uploads.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace backend.Features.Uploads.Application.Services;

public class UploadService : IUploadService
{
    private readonly IUploadRepository _repository;
    // In a real application, you might inject a storage service/provider here.
    // For now, we'll store files locally.
    private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

    public UploadService(IUploadRepository repository)
    {
        _repository = repository;

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<UploadModel?> GetUploadByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<UploadModel>> GetAllUploadsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<UploadModel> ProcessUploadAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null.");
        }

        var uploadId = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{uploadId}{extension}";
        var filePath = Path.Combine(_storagePath, storedFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var uploadModel = new UploadModel
        {
            Id = uploadId,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Size = file.Length,
            StoragePath = filePath,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.AddAsync(uploadModel);
    }

    public async Task DeleteUploadAsync(Guid id)
    {
        var upload = await _repository.GetByIdAsync(id);
        if (upload != null)
        {
            if (File.Exists(upload.StoragePath))
            {
                File.Delete(upload.StoragePath);
            }
            await _repository.DeleteAsync(id);
        }
    }
}
