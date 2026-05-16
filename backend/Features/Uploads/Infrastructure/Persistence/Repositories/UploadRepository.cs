using backend.Features.Uploads.Domain.Entities;
using backend.Features.Uploads.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Uploads.Infrastructure.Persistence.Repositories;

public class UploadRepository : IUploadRepository
{
    private readonly AppDbContext _context;

    public UploadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UploadModel?> GetByIdAsync(Guid id)
    {
        return await _context.Uploads.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<UploadModel>> GetAllAsync()
    {
        return await _context.Uploads.AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<UploadModel> AddAsync(UploadModel upload)
    {
        await _context.Uploads.AddAsync(upload).ConfigureAwait(false);
        return upload;
    }

    public Task UpdateAsync(UploadModel upload)
    {
        _context.Uploads.Update(upload);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var upload = await GetByIdAsync(id);
        if (upload != null)
        {
            _context.Uploads.Remove(upload);
        }
    }
}
