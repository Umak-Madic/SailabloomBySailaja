using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application.Services;
using Sailabloom.Domain;

namespace Sailabloom.Web.Pages;

public class OrderConfirmationModel : PageModel
{
    private readonly IOrderService _orders;

    public OrderConfirmationModel(IOrderService orders)
    {
        _orders = orders;
    }

    public string ZelleNumber => OrderService.ZelleNumber;

    public Order? Order { get; private set; }

    public async Task OnGetAsync(int id)
    {
        Order = await _orders.GetOrderAsync(id);
    }
}
