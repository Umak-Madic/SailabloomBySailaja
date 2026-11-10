namespace Sailabloom.Application.Services;

public interface IReportService
{
    /// <summary>Gross / total refunds / NET across all non-cancelled orders.</summary>
    Task<SalesSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>Month-wise gross, refunds and NET, oldest month first.</summary>
    Task<List<MonthlySalesDto>> GetMonthlySalesAsync(int months = 12, CancellationToken cancellationToken = default);

    /// <summary>Revenue share per plant category.</summary>
    Task<List<CategoryShareDto>> GetCategoryShareAsync(CancellationToken cancellationToken = default);
}
