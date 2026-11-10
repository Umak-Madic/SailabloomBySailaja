namespace Sailabloom.Domain;

/// <summary>A plant for sale in the catalog.</summary>
public class Plant
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    /// <summary>
    /// Units on hand. Configured as an EF Core concurrency token in the
    /// Infrastructure layer, so a stock decrement can never silently
    /// overwrite a competing checkout's decrement.
    /// </summary>
    public int Stock { get; set; }

    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;
}
