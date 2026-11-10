using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application.Services;
using Sailabloom.Domain;

namespace Sailabloom.Web.Pages.Admin.Plants;

public class DeleteModel : PageModel
{
    private readonly ICatalogService _catalog;

    public DeleteModel(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    public Plant? Plant { get; private set; }

    public async Task OnGetAsync(int id)
    {
        Plant = await _catalog.GetPlantAsync(id);
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _catalog.DeletePlantAsync(id);
        return RedirectToPage("/Admin/Plants/Index");
    }
}
