using System.Linq.Expressions;

namespace concerts_gate.server.Repositories.Interfaces;

/// <summary>
/// Generic repository interface providing common data access operations.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IBaseRepository<T> where T : class
{
    /// <summary>
    /// Retrieves an entity by its primary key (GUID).
    /// </summary>
    /// <param name="id">Unique identifier of the entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity if found, otherwise null.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns an IQueryable representing all entities for filtering, sorting, and pagination.
    /// </summary>
    /// <returns>IQueryable of entities.</returns>
    IQueryable<T> GetAll();

    /// <summary>
    /// Queries entities matching a predicate condition.
    /// </summary>
    /// <param name="predicate">Filter predicate expression.</param>
    /// <returns>IQueryable of matching entities.</returns>
    IQueryable<T> Find(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Adds a new entity to the DbContext.
    /// </summary>
    /// <param name="entity">Entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an entity as updated in the DbContext.
    /// </summary>
    /// <param name="entity">Entity to update.</param>
    void Update(T entity);

    /// <summary>
    /// Removes an entity from the DbContext.
    /// </summary>
    /// <param name="entity">Entity to delete.</param>
    void Delete(T entity);

    /// <summary>
    /// Persists all pending context changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
