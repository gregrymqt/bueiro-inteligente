using backend.Core;
using backend.Features.Drains.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DrainEntity = global::backend.Features.Drain.Domain.Drain;

namespace backend.Features.Drains.Infrastructure.Persistence;

// C# 12: Injeção direta via Primary Constructor
public sealed class DrainRepository(AppDbContext dbContext, ILogger<DrainRepository> logger) : IDrainRepository
{
    public async Task<DrainEntity?> GetByIdAsync(Guid drainId, CancellationToken ct = default)
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

    public async Task<DrainEntity?> GetByHardwareIdAsync(string hardwareId, CancellationToken ct = default)
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
            throw new ConnectionException("DrainRepository.GetByHardwareIdAsync", $"Failed to query drain with hardwareId '{hardwareId}'.", ex);
        }
    }

    public async Task<IReadOnlyList<DrainEntity>> GetAllAsync(int skip = 0, int limit = 100, CancellationToken ct = default)
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

    public async Task<DrainEntity> CreateAsync(DrainEntity drain, CancellationToken ct = default)
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
            throw new ConnectionException("DrainRepository.CreateAsync", $"Failed to create drain '{drain.HardwareId}'.", ex);
        }
    }

    public async Task<DrainEntity> UpdateAsync(DrainEntity drain, CancellationToken ct = default)
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

    public async Task DeleteAsync(DrainEntity drain, CancellationToken ct = default)
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

    public async Task<IReadOnlyList<backend.Features.Drains.Application.DTOs.DrainLookupDTO>> GetAvailableDrainsAsync(CancellationToken ct = default)
    {
        try
        {
            return await dbContext.Drains
                .AsNoTracking()
                .Select(d => new backend.Features.Drains.Application.DTOs.DrainLookupDTO(d.HardwareId, d.Name))
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving available drains lookup list");
            throw new ConnectionException("DrainRepository.GetAvailableDrainsAsync", "Failed to query available drains list.", ex);
        }
    }
}