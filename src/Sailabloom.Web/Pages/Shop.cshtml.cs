using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application.Services;
using Sailabloom.Domain;
using Sailabloom.Web.Services;

namespace Sailabloom.Web.Pages;

public class ShopModel : PageModel
{
    private readonly ICatalogService _catalog;
    private readonly CartService _cart;

    public ShopModel(ICatalogService catalog, CartService cart)
    {
        _catalog = catalog;
        _cart = cart;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    public List<Plant> Plants { get; private set; } = new();

    public List<string> Categories { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Categories = await _catalog.GetCategoriesAsync();
        Plants = await _catalog.GetPlantsAsync(Search, Category);
    }

    public async Task<IActionResult> OnPostAddToCartAsync(int plantId)
    {
        var plant = await _catalog.GetPlantAsync(plantId);
        if (plant is not null)
        {
            _cart.AddItem(plant, 1, plant.Stock);
        }

        return RedirectToPage("/Cart");
    }
}
