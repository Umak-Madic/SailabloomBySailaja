namespace Sailabloom.Domain;

/// <summary>How the customer paid (or will pay) for an order.</summary>
public enum PaymentMethod
{
    Card,
    Zelle,
    Cash
}

/// <summary>Payment state shown to the customer and the admin.</summary>
public enum PaymentStatus
{
    Pending,
    PaidDemo,
    PendingVerification,
    DueAtPickup,
    PartiallyRefunded,
    Refunded
}

/// <summary>How the order reaches the customer.</summary>
public enum FulfillmentMethod
{
    StandardShipping,
    PrepaidLabel,
    Pickup
}

/// <summary>Lifecycle of an order in the admin area.</summary>
public enum OrderStatus
{
    Placed,
    Shipped,
    Delivered,
    IssueReported,
    Replaced,
    Refunded,
    Cancelled
}

/// <summary>Why a refund was issued. A reason is always required.</summary>
public enum RefundReason
{
    DeadOrDamagedInTransit,
    CustomerCancellation,
    WrongPlant,
    Other
}

/// <summary>
/// Card refunds are recorded immediately (demo checkout).
/// Zelle refunds start as PendingManualSend until the owner marks the money sent back.
/// </summary>
public enum RefundStatus
{
    Recorded,
    PendingManualSend,
    Completed
}

/// <summary>What the customer reports on the Arrive-Alive guarantee form.</summary>
public enum IssueType
{
    ArrivedDead,
    ArrivedDamaged,
    WrongPlant,
    Other
}
