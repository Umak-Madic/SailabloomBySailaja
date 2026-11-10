using System.Text.Json.Serialization;

namespace Sailabloom.Domain;

/// <summary>
/// A single refund against an order. Refunds are their own records (an order
/// can have several partial refunds); cumulative refunds never exceed the
/// order total. A refund never restocks by itself — restocking is a separate,
/// explicit admin choice, because a dead plant is never restocked.
/// </summary>
public class Refund
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    [JsonIgnore]
    public Order? Order { get; set; }

    public decimal Amount { get; set; }

    public RefundReason Reason { get; set; }

    public string? Note { get; set; }

    /// <summary>The order's original payment method — refunds go back the same way.</summary>
    public PaymentMethod Method { get; set; }

    public RefundStatus Status { get; set; } = RefundStatus.Recorded;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>For Zelle refunds: when the owner marked the money as sent back.</summary>
    public DateTime? CompletedUtc { get; set; }
}
