using Sailabloom.Application.Repositories;
using Sailabloom.Domain;

namespace Sailabloom.Application.Services;

/// <summary>
/// Checkout and order aftercare. The rules here are the owner-approved ones
/// from the live store:
///  - Carts never reserve stock; stock is claimed only at checkout.
///  - The claim is atomic: every line is validated against current stock,
///    then all decrements and the order save happen as one unit of work.
///    Any shortfall rejects the WHOLE order and changes no stock.
///  - Cash forces pickup; standard shipping is $15; an address is required
///    for shipped orders only.
///  - Refunds never restock unless the admin explicitly chooses restock,
///    and restocking happens at most once per order.
/// </summary>
public class OrderService : IOrderService
{
    public const decimal StandardShippingCost = 15m;
    public const string ZelleNumber = "(281) 777-2499";

    private readonly IPlantRepository _plants;
    private readonly IOrderRepository _orders;
    private readonly IRefundRepository _refunds;
    private readonly IIssueReportRepository _issueReports;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IPlantRepository plants,
        IOrderRepository orders,
        IRefundRepository refunds,
        IIssueReportRepository issueReports,
        IUnitOfWork unitOfWork)
    {
        _plants = plants;
        _orders = orders;
        _refunds = refunds;
        _issueReports = issueReports;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderPlacementResult> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Lines.Count == 0)
        {
            return OrderPlacementResult.Fail("Your cart is empty.");
        }

        // Merge duplicate lines for the same plant before claiming stock.
        var merged = request.Lines
            .GroupBy(l => l.PlantId)
            .Select(g => new CartLineDto { PlantId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        if (merged.Any(l => l.Quantity <= 0))
        {
            return OrderPlacementResult.Fail("Quantities must be at least 1.");
        }

        // Cash is always pickup at home.
        var fulfillment = request.PaymentMethod == PaymentMethod.Cash
            ? FulfillmentMethod.Pickup
            : request.Fulfillment;

        if (fulfillment != FulfillmentMethod.Pickup && string.IsNullOrWhiteSpace(request.Address))
        {
            return OrderPlacementResult.Fail("A delivery address is required for shipped orders.");
        }

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var plants = await _plants.GetByIdsAsync(
                    merged.Select(m => m.PlantId).ToList(), cancellationToken);

                if (plants.Count != merged.Count)
                {
                    return OrderPlacementResult.Fail("One or more plants in your cart are no longer available.");
                }

                // Validate EVERY line before changing any stock.
                foreach (var line in merged)
                {
                    var plant = plants.First(p => p.Id == line.PlantId);
                    if (plant.Stock < line.Quantity)
                    {
                        return OrderPlacementResult.Fail(
                            $"Only {plant.Stock} × {plant.Name} left in stock — please reduce the quantity in your cart.");
                    }
                }

                // Claim the stock.
                foreach (var line in merged)
                {
                    plants.First(p => p.Id == line.PlantId).Stock -= line.Quantity;
                }

                var subtotal = merged.Sum(line =>
                    plants.First(p => p.Id == line.PlantId).Price * line.Quantity);
                var shipping = fulfillment == FulfillmentMethod.StandardShipping
                    ? StandardShippingCost
                    : 0m;

                var (paymentStatus, paymentStatusText) = request.PaymentMethod switch
                {
                    PaymentMethod.Card => (PaymentStatus.PaidDemo, "Paid (demo)"),
                    PaymentMethod.Zelle => (PaymentStatus.PendingVerification, "Zelle — payment pending verification"),
                    PaymentMethod.Cash => (PaymentStatus.DueAtPickup, "Cash — due at pickup"),
                    _ => (PaymentStatus.Pending, string.Empty)
                };

                var order = new Order
                {
                    CreatedUtc = DateTime.UtcNow,
                    CustomerName = request.CustomerName.Trim(),
                    Email = request.Email.Trim(),
                    Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = paymentStatus,
                    PaymentStatusText = paymentStatusText,
                    Fulfillment = fulfillment,
                    Subtotal = subtotal,
                    ShippingCost = shipping,
                    Total = subtotal + shipping,
                    Status = OrderStatus.Placed,
                    CardBrand = request.PaymentMethod == PaymentMethod.Card ? request.CardBrand : null,
                    CardLast4 = request.PaymentMethod == PaymentMethod.Card ? request.CardLast4 : null,
                    Items = merged.Select(line =>
                    {
                        var plant = plants.First(p => p.Id == line.PlantId);
                        return new OrderItem
                        {
                            PlantId = plant.Id,
                            PlantName = plant.Name,
                            Category = plant.Category,
                            UnitPrice = plant.Price,
                            Quantity = line.Quantity,
                            LineTotal = plant.Price * line.Quantity
                        };
                    }).ToList()
                };

                await _orders.AddAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OrderPlacementResult.Ok(order.Id, order.Total);
            }, cancellationToken);
        }
        catch (StockConflictException)
        {
            return OrderPlacementResult.Fail(
                "Sorry — stock for one or more plants just changed. Please review your cart and try again.");
        }
    }

    public Task<Order?> GetOrderAsync(int id, CancellationToken cancellationToken = default) =>
        _orders.GetByIdAsync(id, cancellationToken);

    public Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default) =>
        _orders.GetAllAsync(cancellationToken);

    public async Task<ServiceResult> AdvanceStatusAsync(int orderId, OrderStatus newStatus, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return ServiceResult.Fail("Order not found.");
        }

        if (order.Status is OrderStatus.Cancelled or OrderStatus.Refunded)
        {
            return ServiceResult.Fail("This order is closed and its status can no longer change.");
        }

        order.Status = newStatus;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RefundAsync(int orderId, decimal amount, RefundReason reason, string? note, bool restock, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            return ServiceResult.Fail("Enter a refund amount greater than zero.");
        }

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var order = await _orders.GetByIdAsync(orderId, cancellationToken);
                if (order is null)
                {
                    return ServiceResult.Fail("Order not found.");
                }

                if (order.Status == OrderStatus.Cancelled)
                {
                    return ServiceResult.Fail("This order is already cancelled.");
                }

                var alreadyRefunded = order.Refunds.Sum(r => r.Amount);
                if (alreadyRefunded + amount > order.Total)
                {
                    return ServiceResult.Fail(
                        $"That refund exceeds the order total. You can refund up to {order.Total - alreadyRefunded:C} more on this order.");
                }

                if (restock && order.Restocked)
                {
                    return ServiceResult.Fail("This order's items were already restocked — an order can be restocked only once.");
                }

                var refund = new Refund
                {
                    OrderId = order.Id,
                    Amount = amount,
                    Reason = reason,
                    Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                    Method = order.PaymentMethod,
                    Status = order.PaymentMethod == PaymentMethod.Zelle
                        ? RefundStatus.PendingManualSend
                        : RefundStatus.Recorded,
                    CreatedUtc = DateTime.UtcNow
                };
                await _refunds.AddAsync(refund, cancellationToken);

                if (restock)
                {
                    await RestockAsync(order, cancellationToken);
                    order.Restocked = true;
                }

                var totalRefunded = alreadyRefunded + amount;
                if (totalRefunded >= order.Total)
                {
                    order.PaymentStatus = PaymentStatus.Refunded;
                    order.PaymentStatusText = "Refunded";
                    order.Status = OrderStatus.Refunded;
                }
                else
                {
                    order.PaymentStatus = PaymentStatus.PartiallyRefunded;
                    order.PaymentStatusText = $"Partially refunded ({totalRefunded:C} of {order.Total:C})";
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return ServiceResult.Ok();
            }, cancellationToken);
        }
        catch (StockConflictException)
        {
            return ServiceResult.Fail("Stock changed while processing this refund. Please try again.");
        }
    }

    public async Task<ServiceResult> MarkRefundSentAsync(int refundId, CancellationToken cancellationToken = default)
    {
        var refund = await _refunds.GetByIdAsync(refundId, cancellationToken);
        if (refund is null)
        {
            return ServiceResult.Fail("Refund not found.");
        }

        refund.Status = RefundStatus.Completed;
        refund.CompletedUtc = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ReplaceAsync(int orderId, int orderItemId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            return ServiceResult.Fail("Enter a replacement quantity of at least 1.");
        }

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var order = await _orders.GetByIdAsync(orderId, cancellationToken);
                if (order is null)
                {
                    return ServiceResult.Fail("Order not found.");
                }

                var item = order.Items.FirstOrDefault(i => i.Id == orderItemId);
                if (item is null)
                {
                    return ServiceResult.Fail("That item is not part of this order.");
                }

                var plant = await _plants.GetByIdAsync(item.PlantId, cancellationToken);
                if (plant is null)
                {
                    return ServiceResult.Fail("The original plant is no longer in the catalog.");
                }

                if (plant.Stock < quantity)
                {
                    return ServiceResult.Fail(
                        $"Only {plant.Stock} × {plant.Name} in stock — not enough for a replacement. Consider a refund instead.");
                }

                plant.Stock -= quantity;

                order.Replacements.Add(new Replacement
                {
                    OrderId = order.Id,
                    OrderItemId = item.Id,
                    PlantId = plant.Id,
                    PlantName = plant.Name,
                    Quantity = quantity,
                    CreatedUtc = DateTime.UtcNow
                });
                order.Status = OrderStatus.Replaced;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return ServiceResult.Ok();
            }, cancellationToken);
        }
        catch (StockConflictException)
        {
            return ServiceResult.Fail("Stock changed while processing this replacement. Please try again.");
        }
    }

    public async Task<ServiceResult> CancelOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var order = await _orders.GetByIdAsync(orderId, cancellationToken);
                if (order is null)
                {
                    return ServiceResult.Fail("Order not found.");
                }

                if (order.Status == OrderStatus.Cancelled)
                {
                    return ServiceResult.Fail("This order is already cancelled.");
                }

                if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered)
                {
                    return ServiceResult.Fail("Only orders that have not shipped can be cancelled.");
                }

                // An unshipped order's plants go back on the shelf.
                if (!order.Restocked)
                {
                    await RestockAsync(order, cancellationToken);
                    order.Restocked = true;
                }

                // Record any money still owed back (cash pickup orders were never paid).
                var alreadyRefunded = order.Refunds.Sum(r => r.Amount);
                var owed = order.Total - alreadyRefunded;
                if (owed > 0 && order.PaymentMethod != PaymentMethod.Cash)
                {
                    await _refunds.AddAsync(new Refund
                    {
                        OrderId = order.Id,
                        Amount = owed,
                        Reason = RefundReason.CustomerCancellation,
                        Note = "Order cancelled before shipping.",
                        Method = order.PaymentMethod,
                        Status = order.PaymentMethod == PaymentMethod.Zelle
                            ? RefundStatus.PendingManualSend
                            : RefundStatus.Recorded,
                        CreatedUtc = DateTime.UtcNow
                    }, cancellationToken);

                    order.PaymentStatus = PaymentStatus.Refunded;
                    order.PaymentStatusText = "Refunded (cancelled)";
                }

                order.Status = OrderStatus.Cancelled;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return ServiceResult.Ok();
            }, cancellationToken);
        }
        catch (StockConflictException)
        {
            return ServiceResult.Fail("Stock changed while cancelling this order. Please try again.");
        }
    }

    public async Task<ServiceResult> SubmitIssueReportAsync(int orderId, string email, IssueType issueType, string description, string? photoFileName, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(orderId, cancellationToken);

        // One generic error for both "no such order" and "email mismatch" —
        // the form must never reveal whether an order number exists.
        if (order is null || !string.Equals(order.Email, email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult.Fail("We couldn't match that order number and email. Please check both and try again.");
        }

        await _issueReports.AddAsync(new IssueReport
        {
            OrderId = order.Id,
            ContactEmail = order.Email,
            IssueType = issueType,
            Description = description.Trim(),
            PhotoFileName = photoFileName,
            Status = "Open",
            CreatedUtc = DateTime.UtcNow
        }, cancellationToken);

        order.Status = OrderStatus.IssueReported;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public Task<List<IssueReport>> GetIssueReportsAsync(CancellationToken cancellationToken = default) =>
        _issueReports.GetAllAsync(cancellationToken);

    private async Task RestockAsync(Order order, CancellationToken cancellationToken)
    {
        var plantIds = order.Items.Select(i => i.PlantId).Distinct().ToList();
        var plants = await _plants.GetByIdsAsync(plantIds, cancellationToken);

        foreach (var item in order.Items)
        {
            var plant = plants.FirstOrDefault(p => p.Id == item.PlantId);
            if (plant is not null)
            {
                plant.Stock += item.Quantity;
            }
        }
    }
}
