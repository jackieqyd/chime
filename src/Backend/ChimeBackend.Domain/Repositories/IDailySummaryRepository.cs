using ChimeBackend.Domain.Entities;

namespace ChimeBackend.Domain.Repositories;

public interface IDailySummaryRepository
{
    Task<DailySummary?> GetByUserIdAndDateAsync(int userId, DateTime recordDate, CancellationToken cancellationToken = default);
    Task AddAsync(DailySummary summary, CancellationToken cancellationToken = default);
    void Update(DailySummary summary);
}
