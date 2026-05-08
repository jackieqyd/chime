using ChimeBackend.Domain.Entities;

namespace ChimeBackend.Domain.Repositories;

public interface IFoodCategoryRepository
{
    Task<IReadOnlyList<FoodCategory>> GetAllAsync(CancellationToken cancellationToken = default);
}
