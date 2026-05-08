using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Enums;
using ChimeBackend.Domain.Repositories;
using ChimeBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChimeBackend.Infrastructure.Repositories;

public class FoodRecordRepository : IFoodRecordRepository
{
    private readonly ChimeDbContext _dbContext;

    public FoodRecordRepository(ChimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FoodRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodRecords
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<FoodRecord>> GetByUserIdAndDateAsync(
        int userId,
        DateTime recordDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodRecords
            .Where(r => r.UserId == userId && r.RecordDate == recordDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FoodRecord>> QueryAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        MealType? mealType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.FoodRecords
            .Where(r => r.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(r => r.RecordDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(r => r.RecordDate <= endDate.Value);
        if (mealType.HasValue)
            query = query.Where(r => r.MealType == mealType.Value);

        return await query
            .OrderByDescending(r => r.RecordDate)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        MealType? mealType,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.FoodRecords
            .Where(r => r.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(r => r.RecordDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(r => r.RecordDate <= endDate.Value);
        if (mealType.HasValue)
            query = query.Where(r => r.MealType == mealType.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(FoodRecord record, CancellationToken cancellationToken = default)
    {
        await _dbContext.FoodRecords.AddAsync(record, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<FoodRecord> records, CancellationToken cancellationToken = default)
    {
        await _dbContext.FoodRecords.AddRangeAsync(records, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Update(FoodRecord record)
    {
        _dbContext.FoodRecords.Update(record);
    }

    public void Remove(FoodRecord record)
    {
        _dbContext.FoodRecords.Remove(record);
    }

    public async Task RemoveRangeAsync(int userId, DateTime recordDate, MealType mealType, CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.FoodRecords
            .Where(r => r.UserId == userId && r.RecordDate == recordDate && r.MealType == mealType)
            .ToListAsync(cancellationToken);
        _dbContext.FoodRecords.RemoveRange(records);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
