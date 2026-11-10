using System.Text.Json;
using Sailabloom.Domain;

namespace Sailabloom.Web.Services;

/// <summary>One line of the session cart (display snapshot; checkout re-prices from the database).</summary>
public class CartItem
{
    public int PlantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}

/// <summary>
/// Session-based cart. A cart never reserves stock — quantities are capped
/// at the plant's current stock for display, and the real, race-safe stock
/// claim happens in OrderService at checkout.
/// </summary>
public class CartService
{
    private const string SessionKey = "SailabloomCart";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession? Session => _httpContextAccessor.HttpContext?.Session;

    public List<CartItem> GetItems()
    {
        var session = Session;
        if (session is null)
        {
            return new List<CartItem>();
        }

        var json = session.GetString(SessionKey);
        if (json is null)
        {
            return new List<CartItem>();
        }

        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
    }

    public int GetCount() => GetItems().Sum(i => i.Quantity);

    public decimal GetSubtotal() => GetItems().Sum(i => i.Price * i.Quantity);

    /// <summary>Adds up to the available stock — never more than the plant has.</summary>
    public void AddItem(Plant plant, int quantity, int maxStock)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(i => i.PlantId == plant.Id);
        var current = existing?.Quantity ?? 0;
        var allowed = Math.Max(0, Math.Min(quantity, maxStock - current));

        if (allowed == 0)
        {
            return;
        }

        if (existing is null)
        {
            items.Add(new CartItem
            {
                PlantId = plant.Id,
                Name = plant.Name,
                Price = plant.Price,
                Quantity = allowed,
                ImageUrl = plant.ImageUrl
            });
        }
        else
        {
            existing.Quantity += allowed;
        }

        Save(items);
    }

    public void SetQuantity(int plantId, int quantity, int maxStock)
    {
        var items = GetItems();
        var item = items.FirstOrDefault(i => i.PlantId == plantId);
        if (item is null)
        {
            return;
        }

        item.Quantity = Math.Clamp(quantity, 0, Math.Max(0, maxStock));
        if (item.Quantity == 0)
        {
            items.Remove(item);
        }

        Save(items);
    }

    public void RemoveItem(int plantId)
    {
        var items = GetItems();
        items.RemoveAll(i => i.PlantId == plantId);
        Save(items);
    }

    public void Clear() => Session?.Remove(SessionKey);

    private void Save(List<CartItem> items)
    {
        Session?.SetString(SessionKey, JsonSerializer.Serialize(items));
    }
}
