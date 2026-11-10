using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application;
using Sailabloom.Application.Services;
using Sailabloom.Domain;

namespace Sailabloom.Web.Pages.Admin.Orders;

public class DetailsModel : PageModel
{
    private readonly IOrderService _orders;

    public DetailsModel(IOrderService orders)
    {
        _orders = orders;
    }

    public Order? Order { get; private set; }

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? Error { get; set; }

    public async Task OnGetAsync(int id)
    {
        Order = await _orders.GetOrderAsync(id);
    }

    public async Task<IActionResult> OnPostAdvanceAsync(int id, OrderStatus newStatus)
    {
        return await ApplyAsync(id, () => _orders.AdvanceStatusAsync(id, newStatus), "Order status updated.");
    }

    public async Task<IActionResult> OnPostRefundAsync(int id, decimal refundAmount, RefundReason refundReason, string? refundNote, bool restock)
    {
        return await ApplyAsync(id,
            () => _orders.RefundAsync(id, refundAmount, refundReason, refundNote, restock),
            "Refund recorded.");
    }

    public async Task<IActionResult> OnPostReplaceAsync(int id, int orderItemId, int quantity)
    {
        return await ApplyAsync(id,
            () => _orders.ReplaceAsync(id, orderItemId, quantity),
            "Replacement claimed from stock.");
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        return await ApplyAsync(id,
            () => _orders.CancelOrderAsync(id),
            "Order cancelled and items restocked.");
    }

    public async Task<IActionResult> OnPostMarkSentAsync(int id, int refundId)
    {
        return await ApplyAsync(id,
            () => _orders.MarkRefundSentAsync(refundId),
            "Zelle refund marked as sent.");
    }

    private async Task<IActionResult> ApplyAsync(int id, Func<Task<ServiceResult>> action, string successMessage)
    {
        var result = await action();
        if (result.Success)
        {
            Message = successMessage;
        }
        else
        {
            Error = result.Error ?? "Something went wrong.";
        }

        return RedirectToPage(new { id });
    }
}
