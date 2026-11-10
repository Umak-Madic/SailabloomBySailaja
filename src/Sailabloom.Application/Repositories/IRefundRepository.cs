using Sailabloom.Domain;

namespace Sailabloom.Application.Repositories;

public interface IRefundRepository
{
    Task<List<Refund>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<Refund>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);

    Task<Refund?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Refund refund, CancellationToken cancellationToken = default);
}
