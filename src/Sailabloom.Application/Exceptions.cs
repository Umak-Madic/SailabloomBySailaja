namespace Sailabloom.Application;

/// <summary>
/// Raised by the Infrastructure unit of work when a competing transaction
/// changed stock first (EF Core optimistic-concurrency conflict). Services
/// translate it into a friendly "stock just changed" failure so the
/// Application layer never depends on EF Core types.
/// </summary>
public class StockConflictException : Exception
{
    public StockConflictException(string message) : base(message)
    {
    }
}
