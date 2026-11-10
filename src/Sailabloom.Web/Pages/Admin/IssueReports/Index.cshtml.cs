using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application.Services;
using Sailabloom.Domain;

namespace Sailabloom.Web.Pages.Admin.IssueReports;

public class IndexModel : PageModel
{
    private readonly IOrderService _orders;

    public IndexModel(IOrderService orders)
    {
        _orders = orders;
    }

    public List<IssueReport> Reports { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Reports = await _orders.GetIssueReportsAsync();
    }
}
