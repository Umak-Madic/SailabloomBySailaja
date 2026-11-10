using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application.Services;
using Sailabloom.Domain;
using Sailabloom.Web.Services;

namespace Sailabloom.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ICatalogService _catalog;
    private readonly CartService _cart;

    public IndexModel(ICatalogService catalog, CartService cart)
    {
        _catalog = catalog;
        _cart = cart;
    }

    public List<Plant> Featured { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var plants = await _catalog.GetPlantsAsync(null, null);
        Featured = plants.Take(4).ToList();
    }

    public async Task<IActionResult> OnPostAddToCartAsync(int plantId)
    {
        var plant = await _catalog.GetPlantAsync(plantId);
        if (plant is not null)
        {
            _cart.AddItem(plant, 1, plant.Stock);
        }

        return RedirectToPage("/Index");
    }
}
