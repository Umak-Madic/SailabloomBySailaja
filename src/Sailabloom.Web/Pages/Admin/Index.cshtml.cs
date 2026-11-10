using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application;
using Sailabloom.Application.Services;

namespace Sailabloom.Web.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly IReportService _reports;
    private readonly IOrderService _orders;

    public IndexModel(IReportService reports, IOrderService orders)
    {
        _reports = reports;
        _orders = orders;
    }

    public SalesSummaryDto Summary { get; private set; } = new();

    public int OpenIssueReports { get; private set; }

    public async Task OnGetAsync()
    {
        Summary = await _reports.GetSummaryAsync();
        var issueReports = await _orders.GetIssueReportsAsync();
        OpenIssueReports = issueReports.Count(r => r.Status == "Open");
    }
}
