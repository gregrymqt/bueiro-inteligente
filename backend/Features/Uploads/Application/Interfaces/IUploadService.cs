using backend.Features.Uploads.Domain;
using backend.Features.Uploads.Domain.Entities;

namespace backend.Features.Uploads.Application.Interfaces;

public interface IUploadService
{
    Task<UploadModel?> GetUploadByIdAsync(Guid id);
    Task<IEnumerable<UploadModel>> GetAllUploadsAsync();
    Task<UploadModel> ProcessUploadAsync(IFormFile file);
    Task DeleteUploadAsync(Guid id);
}
