using Microsoft.EntityFrameworkCore;
using Sailabloom.Application.Repositories;
using Sailabloom.Domain;
using Sailabloom.Infrastructure.Data;

namespace Sailabloom.Infrastructure.Repositories;

public class IssueReportRepository : IIssueReportRepository
{
    private readonly SailabloomDbContext _context;

    public IssueReportRepository(SailabloomDbContext context)
    {
        _context = context;
    }

    public Task<List<IssueReport>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.IssueReports
            .Include(r => r.Order)
            .OrderByDescending(r => r.CreatedUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(IssueReport report, CancellationToken cancellationToken = default) =>
        await _context.IssueReports.AddAsync(report, cancellationToken);
}
