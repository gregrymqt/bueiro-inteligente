namespace backend.Infrastructure.Persistence;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken ct = default);
    Task ExecuteTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken ct = default
    );
    Task<T> ExecuteTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct = default
    );
}
