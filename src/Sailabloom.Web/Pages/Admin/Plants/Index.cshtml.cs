using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application.Services;
using Sailabloom.Domain;

namespace Sailabloom.Web.Pages.Admin.Plants;

public class IndexModel : PageModel
{
    private readonly ICatalogService _catalog;

    public IndexModel(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    public List<Plant> Plants { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Plants = await _catalog.GetPlantsAsync(null, null);
    }
}
