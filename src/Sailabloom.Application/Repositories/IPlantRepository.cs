using Sailabloom.Domain;

namespace Sailabloom.Application.Repositories;

public interface IPlantRepository
{
    Task<List<Plant>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Plant?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Returns tracked plants, for stock changes inside a transaction.</summary>
    Task<List<Plant>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);

    Task AddAsync(Plant plant, CancellationToken cancellationToken = default);

    void Update(Plant plant);

    void Remove(Plant plant);
}
