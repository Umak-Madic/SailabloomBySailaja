using Microsoft.EntityFrameworkCore;
using Sailabloom.Application.Repositories;
using Sailabloom.Domain;
using Sailabloom.Infrastructure.Data;

namespace Sailabloom.Infrastructure.Repositories;

public class RefundRepository : IRefundRepository
{
    private readonly SailabloomDbContext _context;

    public RefundRepository(SailabloomDbContext context)
    {
        _context = context;
    }

    public Task<List<Refund>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Refunds
            .OrderByDescending(r => r.CreatedUtc)
            .ToListAsync(cancellationToken);

    public Task<List<Refund>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default) =>
        _context.Refunds
            .Where(r => r.OrderId == orderId)
            .OrderByDescending(r => r.CreatedUtc)
            .ToListAsync(cancellationToken);

    public Task<Refund?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Refunds.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task AddAsync(Refund refund, CancellationToken cancellationToken = default) =>
        await _context.Refunds.AddAsync(refund, cancellationToken);
}
