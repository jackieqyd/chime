using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Repositories;
using ChimeBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChimeBackend.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ChimeDbContext _dbContext;

    public UserRepository(ChimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<User?> GetByOpenIdAsync(string openId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.OpenId == openId, cancellationToken);
    }

    public async Task<User?> GetByUnionIdAsync(string unionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.UnionId == unionId, cancellationToken);
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
