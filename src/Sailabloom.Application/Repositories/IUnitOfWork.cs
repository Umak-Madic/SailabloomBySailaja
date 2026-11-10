namespace Sailabloom.Application.Repositories;

/// <summary>
/// Coordinates saves across repositories and runs multi-step operations
/// inside one database transaction on relational providers (serializable
/// for stock claims). On the InMemory provider the action simply runs —
/// services validate everything before mutating and save exactly once.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="action"/> atomically. A stock concurrency
    /// conflict surfaces as <see cref="StockConflictException"/>.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
}
