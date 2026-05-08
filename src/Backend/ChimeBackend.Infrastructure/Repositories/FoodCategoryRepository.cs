using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Repositories;
using ChimeBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChimeBackend.Infrastructure.Repositories;

public class FoodCategoryRepository : IFoodCategoryRepository
{
    private readonly FoodLibraryDbContext _dbContext;

    public FoodCategoryRepository(FoodLibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<FoodCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodCategories
            .OrderBy(c => c.CateId)
            .ToListAsync(cancellationToken);
    }
}
