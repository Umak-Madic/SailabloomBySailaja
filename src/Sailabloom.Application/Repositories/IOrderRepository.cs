using Sailabloom.Domain;

namespace Sailabloom.Application.Repositories;

public interface IOrderRepository
{
    /// <summary>All orders with items, refunds and replacements, newest first.</summary>
    Task<List<Order>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>One order with items, refunds and replacements, or null.</summary>
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Order order, CancellationToken cancellationToken = default);
}
