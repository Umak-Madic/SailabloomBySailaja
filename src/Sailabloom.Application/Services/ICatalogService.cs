using Sailabloom.Domain;

namespace Sailabloom.Application.Services;

public interface ICatalogService
{
    Task<List<Plant>> GetPlantsAsync(string? search, string? category, CancellationToken cancellationToken = default);

    Task<Plant?> GetPlantAsync(int id, CancellationToken cancellationToken = default);

    Task<List<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task<Plant> CreatePlantAsync(Plant plant, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdatePlantAsync(Plant plant, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeletePlantAsync(int id, CancellationToken cancellationToken = default);
}
