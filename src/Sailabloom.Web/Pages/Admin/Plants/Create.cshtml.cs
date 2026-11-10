using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application.Services;

namespace Sailabloom.Web.Pages.Admin.Plants;

public class CreateModel : PageModel
{
    private readonly ICatalogService _catalog;

    public CreateModel(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    [BindProperty]
    public PlantInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _catalog.CreatePlantAsync(Input.ToPlant());
        return RedirectToPage("/Admin/Plants/Index");
    }
}
