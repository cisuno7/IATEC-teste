using AgendaManager.Domain.Entities;
using AgendaManager.Domain.Interfaces;
using AgendaManager.Domain.ValueObjects;
using AgendaManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgendaManager.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(Email email)
    {
        if (email is null)
            throw new ArgumentNullException(nameof(email));

        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == email.Value);
    }

    public async Task<bool> ExistsByEmailAsync(Email email)
    {
        if (email is null)
            throw new ArgumentNullException(nameof(email));

        return await _context.Users
            .AnyAsync(u => u.Email.Value == email.Value);
    }

    public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> userIds)
    {
        if (userIds == null)
            throw new ArgumentNullException(nameof(userIds));

        var ids = userIds.Where(id => id != Guid.Empty).ToList();

        if (!ids.Any())
            return Enumerable.Empty<User>();

        return await _context.Users
            .Where(u => ids.Contains(u.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }
}
