using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace backend.Features.Uploads.Domain.Interfaces;

public interface IUploadService
{
    Task<UploadModel?> GetUploadByIdAsync(Guid id);
    Task<IEnumerable<UploadModel>> GetAllUploadsAsync();
    Task<UploadModel> ProcessUploadAsync(IFormFile file);
    Task DeleteUploadAsync(Guid id);
}
