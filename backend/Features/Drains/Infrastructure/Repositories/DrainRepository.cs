using backend.Core;
using backend.Features.Drains.Domain.Entities;
using backend.Features.Drains.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Drains.Infrastructure.Repositories;

// C# 12: Injeção direta via Primary Constructor
public sealed class DrainRepository(AppDbContext dbContext, ILogger<DrainRepository> logger) : IDrainRepository
{
    public async Task<Drain?> GetByIdAsync(Guid drainId, CancellationToken ct = default)
    {
        try
        {
            return await dbContext.Drains
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == drainId, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving drain by id {DrainId}", drainId);
            throw new ConnectionException("DrainRepository.GetByIdAsync", $"Failed to query drain '{drainId}'.", ex);
        }
    }

    public async Task<Drain?> GetByHardwareIdAsync(string hardwareId, CancellationToken ct = default)
    {
        try
        {
            return await dbContext.Drains
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.HardwareId == hardwareId, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving drain by hardwareId {HardwareId}", hardwareId);
            throw new ConnectionException("DrainRepository.GetByHardwareIdAsync",
                $"Failed to query drain with hardwareId '{hardwareId}'.", ex);
        }
    }

    public async Task<IReadOnlyList<Drain>> GetAllAsync(int skip = 0, int limit = 100, CancellationToken ct = default)
    {
        try
        {
            return await dbContext.Drains
                .AsNoTracking()
                .OrderByDescending(d => d.CreatedAt)
                .Skip(skip)
                .Take(limit)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving drains list");
            throw new ConnectionException("DrainRepository.GetAllAsync", "Failed to query drains list.", ex);
        }
    }

    public async Task<Drain> CreateAsync(Drain drain, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(drain);
        try
        {
            await dbContext.Drains.AddAsync(drain, ct).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return drain;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating drain {HardwareId}", drain.HardwareId);
            throw new ConnectionException("DrainRepository.CreateAsync",
                $"Failed to create drain '{drain.HardwareId}'.", ex);
        }
    }

    public async Task<Drain> UpdateAsync(Drain drain, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(drain);
        try
        {
            dbContext.Drains.Update(drain);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return drain;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating drain {DrainId}", drain.Id);
            throw new ConnectionException("DrainRepository.UpdateAsync", $"Failed to update drain '{drain.Id}'.", ex);
        }
    }

    public async Task DeleteAsync(Drain drain, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(drain);
        try
        {
            dbContext.Drains.Remove(drain);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting drain {DrainId}", drain.Id);
            throw new ConnectionException("DrainRepository.DeleteAsync", $"Failed to delete drain '{drain.Id}'.", ex);
        }
    }
}
