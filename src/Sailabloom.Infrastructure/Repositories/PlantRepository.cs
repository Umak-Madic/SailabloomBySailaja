using Microsoft.EntityFrameworkCore;
using Sailabloom.Application.Repositories;
using Sailabloom.Domain;
using Sailabloom.Infrastructure.Data;

namespace Sailabloom.Infrastructure.Repositories;

public class PlantRepository : IPlantRepository
{
    private readonly SailabloomDbContext _context;

    public PlantRepository(SailabloomDbContext context)
    {
        _context = context;
    }

    public Task<List<Plant>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Plants
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public Task<Plant?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Plants.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<List<Plant>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) =>
        _context.Plants
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Plant plant, CancellationToken cancellationToken = default) =>
        await _context.Plants.AddAsync(plant, cancellationToken);

    public void Update(Plant plant) => _context.Plants.Update(plant);

    public void Remove(Plant plant) => _context.Plants.Remove(plant);
}
