using Microsoft.EntityFrameworkCore.Storage;

namespace BueiroInteligente.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    private readonly AppDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using IDbContextTransaction transaction = await _dbContext
            .Database.BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            await operation(cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<T> ExecuteTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using IDbContextTransaction transaction = await _dbContext
            .Database.BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            T result = await operation(cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}
