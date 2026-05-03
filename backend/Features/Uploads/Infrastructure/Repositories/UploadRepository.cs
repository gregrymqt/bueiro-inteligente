using backend.Features.Uploads.Domain.Entities;
using backend.Features.Uploads.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Uploads.Infrastructure.Repositories;

public class UploadRepository : IUploadRepository
{
    private readonly AppDbContext _context;

    public UploadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UploadModel?> GetByIdAsync(Guid id)
    {
        return await _context.Set<UploadModel>().FindAsync(id);
    }

    public async Task<IEnumerable<UploadModel>> GetAllAsync()
    {
        return await _context.Set<UploadModel>().ToListAsync();
    }

    public async Task<UploadModel> AddAsync(UploadModel upload)
    {
        await _context.Set<UploadModel>().AddAsync(upload);
        await _context.SaveChangesAsync();
        return upload;
    }

    public async Task UpdateAsync(UploadModel upload)
    {
        _context.Set<UploadModel>().Update(upload);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var upload = await GetByIdAsync(id);
        if (upload != null)
        {
            _context.Set<UploadModel>().Remove(upload);
            await _context.SaveChangesAsync();
        }
    }
}
