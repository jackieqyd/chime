using ChimeBackend.Domain.Entities;

namespace ChimeBackend.Domain.Repositories;

public interface IFoodRepository
{
    Task<Food?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Food>> SearchAsync(string? keyword, int? categoryId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? keyword, int? categoryId, CancellationToken cancellationToken = default);
}
