using Microsoft.AspNetCore.Mvc;
using Sailabloom.Application.Services;
using Sailabloom.Domain;

namespace Sailabloom.Api.Controllers;

/// <summary>
/// Admin plant CRUD over the API.
/// NOTE (development scaffold): these endpoints are not authenticated yet.
/// The Razor Pages Admin area IS protected by Identity + the Admin role;
/// secure these endpoints (e.g. JWT + Admin policy) before exposing the
/// API publicly.
/// </summary>
[ApiController]
[Route("api/admin/plants")]
public class AdminPlantsController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public AdminPlantsController(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Plant plant)
    {
        var created = await _catalog.CreatePlantAsync(plant);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Plant plant)
    {
        plant.Id = id;
        var result = await _catalog.UpdatePlantAsync(plant);
        return result.Success ? NoContent() : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _catalog.DeletePlantAsync(id);
        return result.Success ? NoContent() : BadRequest(result);
    }
}
