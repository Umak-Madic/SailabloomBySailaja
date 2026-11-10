using System.Text.Json.Serialization;

namespace Sailabloom.Domain;

/// <summary>A customer order, created only after its stock claim succeeds.</summary>
public class Order
{
    public int Id { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public string CustomerName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>Delivery address. Optional only when Fulfillment is Pickup.</summary>
    public string? Address { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    /// <summary>Customer-facing payment label, e.g. "Zelle — payment pending verification".</summary>
    public string PaymentStatusText { get; set; } = string.Empty;

    public FulfillmentMethod Fulfillment { get; set; }

    public decimal Subtotal { get; set; }

    /// <summary>Shipping charged on this order (0 for prepaid label or pickup).</summary>
    public decimal ShippingCost { get; set; }

    public decimal Total { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Placed;

    /// <summary>Card brand only — a full card number is never stored.</summary>
    public string? CardBrand { get; set; }

    /// <summary>Last 4 digits only — a full card number is never stored.</summary>
    public string? CardLast4 { get; set; }

    /// <summary>True once this order's items have been put back into stock (at most once).</summary>
    public bool Restocked { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public ICollection<Replacement> Replacements { get; set; } = new List<Replacement>();
}

/// <summary>One line of an order, with name/category/price snapshots from order time.</summary>
public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    [JsonIgnore]
    public Order? Order { get; set; }

    public int PlantId { get; set; }

    public string PlantName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}
