using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application.Services;
using Sailabloom.Web.Services;

namespace Sailabloom.Web.Pages;

public class CartModel : PageModel
{
    private readonly CartService _cart;
    private readonly ICatalogService _catalog;

    public CartModel(CartService cart, ICatalogService catalog)
    {
        _cart = cart;
        _catalog = catalog;
    }

    public List<CartItem> Items { get; private set; } = new();

    public Dictionary<int, int> StockByPlant { get; private set; } = new();

    public decimal Subtotal => Items.Sum(i => i.Price * i.Quantity);

    public async Task OnGetAsync()
    {
        Items = _cart.GetItems();
        foreach (var item in Items)
        {
            var plant = await _catalog.GetPlantAsync(item.PlantId);
            StockByPlant[item.PlantId] = plant?.Stock ?? 0;
        }
    }

    public async Task<IActionResult> OnPostUpdateAsync(int plantId, int quantity)
    {
        var plant = await _catalog.GetPlantAsync(plantId);
        _cart.SetQuantity(plantId, quantity, plant?.Stock ?? 0);
        return RedirectToPage();
    }

    public IActionResult OnPostRemove(int plantId)
    {
        _cart.RemoveItem(plantId);
        return RedirectToPage();
    }

    public IActionResult OnPostClear()
    {
        _cart.Clear();
        return RedirectToPage();
    }
}
