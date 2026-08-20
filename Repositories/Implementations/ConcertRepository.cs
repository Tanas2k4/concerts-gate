using Microsoft.EntityFrameworkCore;
using concerts_gate.server.Common.Enums;
using concerts_gate.server.Data;
using concerts_gate.server.Entities;
using concerts_gate.server.Repositories.Interfaces;

namespace concerts_gate.server.Repositories.Implementations;

/// <summary>
/// Repository implementation for Concert data operations.
/// </summary>
public class ConcertRepository : BaseRepository<Concert>, IConcertRepository
{
    /// <summary>
    /// Initializes a new instance of <see cref="ConcertRepository"/>.
    /// </summary>
    /// <param name="context">Database context.</param>
    public ConcertRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Concert?> GetWithCategoriesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Concerts
            .Include(c => c.TicketCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public IQueryable<Concert> GetPublishedConcerts(string? search = null, string? genre = null, bool? onlyFlashSale = null)
    {
        var query = _context.Concerts
            .Include(c => c.TicketCategories)
            .Where(c => c.Status == ConcertStatus.Published)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(s) ||
                                     c.Artist.ToLower().Contains(s) ||
                                     c.Venue.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            var g = genre.Trim().ToLower();
            query = query.Where(c => c.Genre.ToLower().Contains(g));
        }

        if (onlyFlashSale.HasValue && onlyFlashSale.Value)
        {
            query = query.Where(c => c.IsFlashSale);
        }

        return query.OrderBy(c => c.EventDate);
    }
}

/// <summary>
/// Repository implementation for Ticket Category data operations.
/// </summary>
public class TicketCategoryRepository : BaseRepository<TicketCategory>, ITicketCategoryRepository
{
    /// <summary>
    /// Initializes a new instance of <see cref="TicketCategoryRepository"/>.
    /// </summary>
    /// <param name="context">Database context.</param>
    public TicketCategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<List<TicketCategory>> GetByConcertIdAsync(Guid concertId, CancellationToken cancellationToken = default)
    {
        return await _context.TicketCategories
            .Where(tc => tc.ConcertId == concertId)
            .ToListAsync(cancellationToken);
    }
}
