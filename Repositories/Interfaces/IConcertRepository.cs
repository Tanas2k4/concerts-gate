using concerts_gate.server.Entities;

namespace concerts_gate.server.Repositories.Interfaces;

/// <summary>
/// Repository interface for Concert data access operations.
/// </summary>
public interface IConcertRepository : IBaseRepository<Concert>
{
    /// <summary>
    /// Retrieves a concert along with its ticket categories.
    /// </summary>
    /// <param name="id">Concert unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The <see cref="Concert"/> entity with ticket categories, or null.</returns>
    Task<Concert?> GetWithCategoriesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries published concerts available for search and catalog browsing.
    /// </summary>
    /// <param name="search">Search keyword across title, artist, or venue.</param>
    /// <param name="genre">Music genre filter.</param>
    /// <param name="onlyFlashSale">Filter exclusively for Flash Sale events.</param>
    /// <returns>IQueryable of matching concerts.</returns>
    IQueryable<Concert> GetPublishedConcerts(string? search = null, string? genre = null, bool? onlyFlashSale = null);
}

/// <summary>
/// Repository interface for TicketCategory and inventory operations.
/// </summary>
public interface ITicketCategoryRepository : IBaseRepository<TicketCategory>
{
    /// <summary>
    /// Retrieves all ticket categories for a specific concert.
    /// </summary>
    /// <param name="concertId">Concert unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of <see cref="TicketCategory"/> records.</returns>
    Task<List<TicketCategory>> GetByConcertIdAsync(Guid concertId, CancellationToken cancellationToken = default);
}
