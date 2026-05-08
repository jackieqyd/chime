using ChimeBackend.Domain.Entities;

namespace ChimeBackend.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByOpenIdAsync(string openId, CancellationToken cancellationToken = default);
    Task<User?> GetByUnionIdAsync(string unionId, CancellationToken cancellationToken = default);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
