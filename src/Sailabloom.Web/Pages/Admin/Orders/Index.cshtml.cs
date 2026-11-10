using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application.Services;
using Sailabloom.Domain;

namespace Sailabloom.Web.Pages.Admin.Orders;

public class IndexModel : PageModel
{
    private readonly IOrderService _orders;

    public IndexModel(IOrderService orders)
    {
        _orders = orders;
    }

    public List<Order> Orders { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Orders = await _orders.GetOrdersAsync();
    }
}
