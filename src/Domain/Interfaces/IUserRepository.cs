using AgendaManager.Domain.Entities;
using AgendaManager.Domain.ValueObjects;

namespace AgendaManager.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(Email email);
    Task<bool> ExistsByEmailAsync(Email email);
    Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> userIds);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}