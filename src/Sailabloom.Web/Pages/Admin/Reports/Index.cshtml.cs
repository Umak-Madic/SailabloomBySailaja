using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application;
using Sailabloom.Application.Services;

namespace Sailabloom.Web.Pages.Admin.Reports;

public class IndexModel : PageModel
{
    private readonly IReportService _reports;

    public IndexModel(IReportService reports)
    {
        _reports = reports;
    }

    public SalesSummaryDto Summary { get; private set; } = new();

    public List<MonthlySalesDto> Monthly { get; private set; } = new();

    public List<CategoryShareDto> Categories { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Summary = await _reports.GetSummaryAsync();
        Monthly = await _reports.GetMonthlySalesAsync(12);
        Categories = await _reports.GetCategoryShareAsync();
    }
}
