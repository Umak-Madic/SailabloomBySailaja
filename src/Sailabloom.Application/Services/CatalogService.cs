using Sailabloom.Application.Repositories;
using Sailabloom.Domain;

namespace Sailabloom.Application.Services;

/// <summary>Catalog reads plus admin plant CRUD.</summary>
public class CatalogService : ICatalogService
{
    private readonly IPlantRepository _plants;
    private readonly IUnitOfWork _unitOfWork;

    public CatalogService(IPlantRepository plants, IUnitOfWork unitOfWork)
    {
        _plants = plants;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Plant>> GetPlantsAsync(string? search, string? category, CancellationToken cancellationToken = default)
    {
        var plants = await _plants.GetAllAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            plants = plants
                .Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                            || p.Category.Contains(term, StringComparison.OrdinalIgnoreCase)
                            || p.Description.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            plants = plants
                .Where(p => string.Equals(p.Category, category.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return plants;
    }

    public Task<Plant?> GetPlantAsync(int id, CancellationToken cancellationToken = default) =>
        _plants.GetByIdAsync(id, cancellationToken);

    public async Task<List<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var plants = await _plants.GetAllAsync(cancellationToken);
        return plants
            .Select(p => p.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToList();
    }

    public async Task<Plant> CreatePlantAsync(Plant plant, CancellationToken cancellationToken = default)
    {
        await _plants.AddAsync(plant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return plant;
    }

    public async Task<ServiceResult> UpdatePlantAsync(Plant plant, CancellationToken cancellationToken = default)
    {
        var existing = await _plants.GetByIdAsync(plant.Id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Fail("Plant not found.");
        }

        existing.Name = plant.Name;
        existing.Category = plant.Category;
        existing.Price = plant.Price;
        existing.Stock = plant.Stock;
        existing.Description = plant.Description;
        existing.ImageUrl = plant.ImageUrl;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeletePlantAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _plants.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Fail("Plant not found.");
        }

        _plants.Remove(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }
}
