using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using concerts_gate.server.Data;
using concerts_gate.server.Repositories.Interfaces;

namespace concerts_gate.server.Repositories.Implementations;

/// <summary>
/// Default implementation of <see cref="IBaseRepository{T}"/> using Entity Framework Core.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    /// <summary>
    /// Database context instance.
    /// </summary>
    protected readonly ApplicationDbContext _context;

    /// <summary>
    /// Entity DbSet instance.
    /// </summary>
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of <see cref="BaseRepository{T}"/>.
    /// </summary>
    /// <param name="context">Database context.</param>
    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual IQueryable<T> GetAll()
    {
        return _dbSet.AsQueryable();
    }

    /// <inheritdoc />
    public virtual IQueryable<T> Find(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.Where(predicate);
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <inheritdoc />
    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <inheritdoc />
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
