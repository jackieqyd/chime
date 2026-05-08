using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Repositories;
using ChimeBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChimeBackend.Infrastructure.Repositories;

public class FoodRepository : IFoodRepository
{
    private readonly FoodLibraryDbContext _dbContext;

    public FoodRepository(FoodLibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Food?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Foods
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Food>> SearchAsync(
        string? keyword,
        int? categoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Foods
            .Where(f => f.Status == 1);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(f => f.FoodName.Contains(keyword) ||
                                     (f.AliasName != null && f.AliasName.Contains(keyword)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(f => f.CateId == categoryId.Value);
        }

        return await query
            .OrderBy(f => f.FoodName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? keyword, int? categoryId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Foods
            .Where(f => f.Status == 1);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(f => f.FoodName.Contains(keyword) ||
                                     (f.AliasName != null && f.AliasName.Contains(keyword)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(f => f.CateId == categoryId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}
