using System.Text.Json.Serialization;

namespace Sailabloom.Domain;

/// <summary>
/// A free ($0) replacement shipment for a dead/damaged line, claimed from
/// stock with the same stock rules as checkout. Never counts as revenue.
/// </summary>
public class Replacement
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    [JsonIgnore]
    public Order? Order { get; set; }

    public int OrderItemId { get; set; }

    public int PlantId { get; set; }

    public string PlantName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
