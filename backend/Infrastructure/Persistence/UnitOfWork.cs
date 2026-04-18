using Microsoft.EntityFrameworkCore.Storage;

namespace backend.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    private readonly AppDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public Task<int> CommitAsync(CancellationToken ct = default) => _dbContext.SaveChangesAsync(ct);

    public async Task ExecuteTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var transaction = await _dbContext
            .Database.BeginTransactionAsync(ct)
            .ConfigureAwait(false);

        try
        {
            await operation(ct).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            await transaction.CommitAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<T> ExecuteTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var transaction = await _dbContext
            .Database.BeginTransactionAsync(ct)
            .ConfigureAwait(false);

        try
        {
            T result = await operation(ct).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            await transaction.CommitAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }
    }
}
