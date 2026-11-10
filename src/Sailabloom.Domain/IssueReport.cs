using System.Text.Json.Serialization;

namespace Sailabloom.Domain;

/// <summary>
/// A customer's "Report a problem" submission under the Arrive-Alive
/// guarantee. The order number and checkout email must match the order
/// before a report is accepted.
/// </summary>
public class IssueReport
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    [JsonIgnore]
    public Order? Order { get; set; }

    public string ContactEmail { get; set; } = string.Empty;

    public IssueType IssueType { get; set; }

    public string Description { get; set; } = string.Empty;

    /// <summary>Stored file name of the uploaded photo, if one was attached.</summary>
    public string? PhotoFileName { get; set; }

    public string Status { get; set; } = "Open";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
