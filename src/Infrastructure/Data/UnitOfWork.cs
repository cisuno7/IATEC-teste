using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository? _users;
    private IEventRepository? _events;
    private bool _disposed;

    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IUserRepository Users => _users ??= new Repositories.UserRepository(_context);

    public IEventRepository Events => _events ??= new Repositories.EventRepository(_context);

    public async Task BeginTransactionAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UnitOfWork));

        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UnitOfWork));

        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UnitOfWork));

        await _context.Database.RollbackTransactionAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UnitOfWork));

        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context?.Dispose();
            }

            _users = null;
            _events = null;
            _disposed = true;
        }
    }
}
