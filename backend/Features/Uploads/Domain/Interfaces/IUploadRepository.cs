using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Features.Uploads.Domain.Interfaces;

public interface IUploadRepository
{
    Task<UploadModel?> GetByIdAsync(Guid id);
    Task<IEnumerable<UploadModel>> GetAllAsync();
    Task<UploadModel> AddAsync(UploadModel upload);
    Task UpdateAsync(UploadModel upload);
    Task DeleteAsync(Guid id);
}
