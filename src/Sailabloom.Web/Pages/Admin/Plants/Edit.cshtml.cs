using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application.Services;

namespace Sailabloom.Web.Pages.Admin.Plants;

public class EditModel : PageModel
{
    private readonly ICatalogService _catalog;

    public EditModel(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    [BindProperty]
    public PlantInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var plant = await _catalog.GetPlantAsync(id);
        if (plant is null)
        {
            return RedirectToPage("/Admin/Plants/Index");
        }

        Input = PlantInput.FromPlant(plant);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _catalog.UpdatePlantAsync(Input.ToPlant(id));
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not save this plant.");
            return Page();
        }

        return RedirectToPage("/Admin/Plants/Index");
    }
}
