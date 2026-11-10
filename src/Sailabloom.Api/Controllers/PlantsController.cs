using Microsoft.AspNetCore.Mvc;
using Sailabloom.Application.Services;

namespace Sailabloom.Api.Controllers;

/// <summary>Public catalog endpoints.</summary>
[ApiController]
[Route("api/plants")]
public class PlantsController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public PlantsController(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    /// <summary>List plants, optionally filtered by search text and category.</summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] string? category)
    {
        var plants = await _catalog.GetPlantsAsync(search, category);
        return Ok(plants);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var plant = await _catalog.GetPlantAsync(id);
        return plant is null ? NotFound() : Ok(plant);
    }
}
