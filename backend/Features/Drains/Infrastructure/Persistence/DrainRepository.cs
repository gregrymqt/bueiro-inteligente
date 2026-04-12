using backend.Core;
using backend.Features.Drains.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DrainEntity = global::backend.Features.Drain.Domain.Drain;

namespace backend.Features.Drains.Infrastructure.Persistence;

public sealed class DrainRepository(AppDbContext dbContext, ILogger<DrainRepository> logger)
    : IDrainRepository
{
    private readonly AppDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ILogger<DrainRepository> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<DrainEntity?> GetByIdAsync(
        Guid drainId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await _dbContext
                .Drains.AsNoTracking()
                .FirstOrDefaultAsync(drain => drain.Id == drainId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error retrieving drain by id {DrainId}", drainId);
            throw new ConnectionException(
                "DrainRepository.GetByIdAsync",
                $"Failed to query drain '{drainId}'.",
                exception
            );
        }
    }

    public async Task<DrainEntity?> GetByHardwareIdAsync(
        string hardwareId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await _dbContext
                .Drains.AsNoTracking()
                .FirstOrDefaultAsync(drain => drain.HardwareId == hardwareId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error retrieving drain by hardwareId {HardwareId}",
                hardwareId
            );
            throw new ConnectionException(
                "DrainRepository.GetByHardwareIdAsync",
                $"Failed to query drain with hardwareId '{hardwareId}'.",
                exception
            );
        }
    }

    public async Task<IReadOnlyList<DrainEntity>> GetAllAsync(
        int skip = 0,
        int limit = 100,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await _dbContext
                .Drains.AsNoTracking()
                .Skip(skip)
                .Take(limit)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error retrieving drains list");
            throw new ConnectionException(
                "DrainRepository.GetAllAsync",
                "Failed to query drains list.",
                exception
            );
        }
    }

    public async Task<DrainEntity> CreateAsync(
        DrainEntity drain,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(drain);

        try
        {
            await _dbContext.Drains.AddAsync(drain, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return drain;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error creating drain {HardwareId}", drain.HardwareId);
            throw new ConnectionException(
                "DrainRepository.CreateAsync",
                $"Failed to create drain with hardwareId '{drain.HardwareId}'.",
                exception
            );
        }
    }

    public async Task<DrainEntity> UpdateAsync(
        DrainEntity drain,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(drain);

        try
        {
            _dbContext.Drains.Update(drain);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return drain;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error updating drain {DrainId}", drain.Id);
            throw new ConnectionException(
                "DrainRepository.UpdateAsync",
                $"Failed to update drain '{drain.Id}'.",
                exception
            );
        }
    }

    public async Task DeleteAsync(DrainEntity drain, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(drain);

        try
        {
            _dbContext.Drains.Remove(drain);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error deleting drain {DrainId}", drain.Id);
            throw new ConnectionException(
                "DrainRepository.DeleteAsync",
                $"Failed to delete drain '{drain.Id}'.",
                exception
            );
        }
    }
}
