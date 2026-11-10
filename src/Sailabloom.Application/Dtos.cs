using Sailabloom.Domain;

namespace Sailabloom.Application;

/// <summary>One line of a cart submitted at checkout.</summary>
public class CartLineDto
{
    public int PlantId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>Everything checkout needs. Card data is brand + last 4 only.</summary>
public class PlaceOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Required unless Fulfillment ends up as Pickup.</summary>
    public string? Address { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public FulfillmentMethod Fulfillment { get; set; }

    public string? CardBrand { get; set; }
    public string? CardLast4 { get; set; }

    public List<CartLineDto> Lines { get; set; } = new();
}

/// <summary>Outcome of a checkout attempt.</summary>
public class OrderPlacementResult
{
    public bool Success { get; set; }
    public int? OrderId { get; set; }
    public decimal Total { get; set; }
    public string? Error { get; set; }

    public static OrderPlacementResult Ok(int orderId, decimal total) =>
        new() { Success = true, OrderId = orderId, Total = total };

    public static OrderPlacementResult Fail(string error) =>
        new() { Success = false, Error = error };
}

/// <summary>Outcome of an admin/service operation that does not return data.</summary>
public class ServiceResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }

    public static ServiceResult Ok() => new() { Success = true };

    public static ServiceResult Fail(string error) => new() { Success = false, Error = error };
}

/// <summary>Gross, refunds and NET for one calendar month.</summary>
public class MonthlySalesDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Gross { get; set; }
    public decimal Refunds { get; set; }
    public decimal Net { get; set; }
}

/// <summary>Headline numbers for the admin Sales Reports page.</summary>
public class SalesSummaryDto
{
    public decimal Gross { get; set; }
    public decimal Refunds { get; set; }
    public decimal Net { get; set; }
    public int OrderCount { get; set; }
}

/// <summary>Revenue share of one plant category.</summary>
public class CategoryShareDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percent { get; set; }
}
