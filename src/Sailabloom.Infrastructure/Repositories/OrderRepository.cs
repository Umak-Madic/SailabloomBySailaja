using Microsoft.EntityFrameworkCore;
using Sailabloom.Application.Repositories;
using Sailabloom.Domain;
using Sailabloom.Infrastructure.Data;

namespace Sailabloom.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly SailabloomDbContext _context;

    public OrderRepository(SailabloomDbContext context)
    {
        _context = context;
    }

    public Task<List<Order>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Refunds)
            .Include(o => o.Replacements)
            .OrderByDescending(o => o.CreatedUtc)
            .ToListAsync(cancellationToken);

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Refunds)
            .Include(o => o.Replacements)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
        await _context.Orders.AddAsync(order, cancellationToken);
}
