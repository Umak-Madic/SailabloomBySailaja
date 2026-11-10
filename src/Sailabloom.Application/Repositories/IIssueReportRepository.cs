using Sailabloom.Domain;

namespace Sailabloom.Application.Repositories;

public interface IIssueReportRepository
{
    /// <summary>All reports with their order, newest first.</summary>
    Task<List<IssueReport>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(IssueReport report, CancellationToken cancellationToken = default);
}
