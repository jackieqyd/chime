using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Enums;

namespace ChimeBackend.Domain.Repositories;

public interface IFoodRecordRepository
{
    Task<FoodRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FoodRecord>> GetByUserIdAndDateAsync(int userId, DateTime recordDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FoodRecord>> QueryAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        MealType? mealType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(int userId, DateTime? startDate, DateTime? endDate, MealType? mealType, CancellationToken cancellationToken = default);
    Task AddAsync(FoodRecord record, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<FoodRecord> records, CancellationToken cancellationToken = default);
    void Update(FoodRecord record);
    void Remove(FoodRecord record);
    Task RemoveRangeAsync(int userId, DateTime recordDate, MealType mealType, CancellationToken cancellationToken = default);
}
