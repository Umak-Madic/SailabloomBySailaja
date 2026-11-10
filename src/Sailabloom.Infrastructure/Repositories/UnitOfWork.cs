using System.Data;
using Microsoft.EntityFrameworkCore;
using Sailabloom.Application;
using Sailabloom.Application.Repositories;
using Sailabloom.Infrastructure.Data;

namespace Sailabloom.Infrastructure.Repositories;

/// <summary>
/// EF Core unit of work. On relational providers the action runs inside a
/// serializable transaction (via the execution strategy, which also makes
/// it retry-safe). A DbUpdateConcurrencyException — e.g. a competing
/// checkout decremented the same plant first — is translated into the
/// Application layer's StockConflictException.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly SailabloomDbContext _context;

    public UnitOfWork(SailabloomDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        if (!_context.Database.IsRelational())
        {
            // InMemory: services validate before mutating and save once,
            // so running the action directly is atomic enough for testing.
            try
            {
                return await action();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new StockConflictException("Stock changed while the operation was running.");
            }
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var result = await action();
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new StockConflictException("Stock changed while the operation was running.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
