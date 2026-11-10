using Sailabloom.Domain;

namespace Sailabloom.Application.Services;

public interface IOrderService
{
    Task<OrderPlacementResult> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken cancellationToken = default);

    Task<Order?> GetOrderAsync(int id, CancellationToken cancellationToken = default);

    Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult> AdvanceStatusAsync(int orderId, OrderStatus newStatus, CancellationToken cancellationToken = default);

    Task<ServiceResult> RefundAsync(int orderId, decimal amount, RefundReason reason, string? note, bool restock, CancellationToken cancellationToken = default);

    /// <summary>Marks a Zelle refund (PendingManualSend) as sent back to the customer.</summary>
    Task<ServiceResult> MarkRefundSentAsync(int refundId, CancellationToken cancellationToken = default);

    Task<ServiceResult> ReplaceAsync(int orderId, int orderItemId, int quantity, CancellationToken cancellationToken = default);

    Task<ServiceResult> CancelOrderAsync(int orderId, CancellationToken cancellationToken = default);

    Task<ServiceResult> SubmitIssueReportAsync(int orderId, string email, IssueType issueType, string description, string? photoFileName, CancellationToken cancellationToken = default);

    Task<List<IssueReport>> GetIssueReportsAsync(CancellationToken cancellationToken = default);
}
