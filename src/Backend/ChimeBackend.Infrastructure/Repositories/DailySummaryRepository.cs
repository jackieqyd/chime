using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Repositories;
using ChimeBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChimeBackend.Infrastructure.Repositories;

public class DailySummaryRepository : IDailySummaryRepository
{
    private readonly ChimeDbContext _dbContext;

    public DailySummaryRepository(ChimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DailySummary?> GetByUserIdAndDateAsync(
        int userId,
        DateTime recordDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DailySummaries
            .FirstOrDefaultAsync(s => s.UserId == userId && s.RecordDate == recordDate, cancellationToken);
    }

    public async Task AddAsync(DailySummary summary, CancellationToken cancellationToken = default)
    {
        await _dbContext.DailySummaries.AddAsync(summary, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Update(DailySummary summary)
    {
        _dbContext.DailySummaries.Update(summary);
    }
}
