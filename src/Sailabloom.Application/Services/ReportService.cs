using System.Globalization;
using Sailabloom.Application.Repositories;
using Sailabloom.Domain;

namespace Sailabloom.Application.Services;

/// <summary>
/// Sales reporting. Headline revenue is always NET: gross order totals
/// minus refunds. Cancelled orders are excluded from gross (their refunds,
/// recorded at cancellation, still appear in the refunds total so a
/// cancelled paid order nets correctly in its month).
/// </summary>
public class ReportService : IReportService
{
    private readonly IOrderRepository _orders;
    private readonly IRefundRepository _refunds;

    public ReportService(IOrderRepository orders, IRefundRepository refunds)
    {
        _orders = orders;
        _refunds = refunds;
    }

    public async Task<SalesSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orders.GetAllAsync(cancellationToken);
        var refunds = await _refunds.GetAllAsync(cancellationToken);

        var counted = orders.Where(o => o.Status != OrderStatus.Cancelled).ToList();
        var gross = counted.Sum(o => o.Total);
        var refundTotal = refunds.Sum(r => r.Amount);

        return new SalesSummaryDto
        {
            Gross = gross,
            Refunds = refundTotal,
            Net = gross - refundTotal,
            OrderCount = counted.Count
        };
    }

    public async Task<List<MonthlySalesDto>> GetMonthlySalesAsync(int months = 12, CancellationToken cancellationToken = default)
    {
        var orders = await _orders.GetAllAsync(cancellationToken);
        var refunds = await _refunds.GetAllAsync(cancellationToken);

        var firstMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
            .AddMonths(-(months - 1));

        var result = new List<MonthlySalesDto>();
        for (var i = 0; i < months; i++)
        {
            var monthStart = firstMonth.AddMonths(i);

            var gross = orders
                .Where(o => o.Status != OrderStatus.Cancelled
                            && o.CreatedUtc.Year == monthStart.Year
                            && o.CreatedUtc.Month == monthStart.Month)
                .Sum(o => o.Total);

            var refundTotal = refunds
                .Where(r => r.CreatedUtc.Year == monthStart.Year
                            && r.CreatedUtc.Month == monthStart.Month)
                .Sum(r => r.Amount);

            result.Add(new MonthlySalesDto
            {
                Year = monthStart.Year,
                Month = monthStart.Month,
                MonthLabel = monthStart.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Gross = gross,
                Refunds = refundTotal,
                Net = gross - refundTotal
            });
        }

        return result;
    }

    public async Task<List<CategoryShareDto>> GetCategoryShareAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orders.GetAllAsync(cancellationToken);

        var byCategory = orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SelectMany(o => o.Items)
            .GroupBy(i => i.Category)
            .Select(g => new { Category = g.Key, Amount = g.Sum(i => i.LineTotal) })
            .OrderByDescending(x => x.Amount)
            .ToList();

        var total = byCategory.Sum(x => x.Amount);

        return byCategory
            .Select(x => new CategoryShareDto
            {
                Category = x.Category,
                Amount = x.Amount,
                Percent = total == 0 ? 0 : Math.Round((double)(x.Amount / total * 100), 1)
            })
            .ToList();
    }
}
