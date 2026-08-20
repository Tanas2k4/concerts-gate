using Microsoft.EntityFrameworkCore;
using concerts_gate.server.Common.Enums;
using concerts_gate.server.Data;
using concerts_gate.server.Entities;
using concerts_gate.server.Repositories.Interfaces;

namespace concerts_gate.server.Repositories.Implementations;

/// <summary>
/// Repository implementation for Booking data operations.
/// </summary>
public class BookingRepository : BaseRepository<Booking>, IBookingRepository
{
    /// <summary>
    /// Initializes a new instance of <see cref="BookingRepository"/>.
    /// </summary>
    /// <param name="context">Database context.</param>
    public BookingRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Booking?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Concert)
            .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.TicketCategory)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.TicketCategory)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Booking?> GetByCodeAsync(string bookingCode, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.TicketCategory)
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync(b => b.BookingCode == bookingCode, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<Booking>> GetExpiredPendingBookingsAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.TicketCategory)
            .Where(b => b.Status == BookingStatus.PendingPayment && b.ReservationExpiresAt <= now)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddTicketsAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default)
    {
        await _context.Tickets.AddRangeAsync(tickets, cancellationToken);
    }
}

/// <summary>
/// Repository implementation for Promotional Voucher data operations.
/// </summary>
public class VoucherRepository : BaseRepository<Voucher>, IVoucherRepository
{
    /// <summary>
    /// Initializes a new instance of <see cref="VoucherRepository"/>.
    /// </summary>
    /// <param name="context">Database context.</param>
    public VoucherRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Voucher?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var upperCode = code.Trim().ToUpperInvariant();
        return await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == upperCode, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetUserUsageCountAsync(Guid voucherId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.VoucherUsages
            .CountAsync(vu => vu.VoucherId == voucherId && vu.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddUsageAsync(VoucherUsage usage, CancellationToken cancellationToken = default)
    {
        await _context.VoucherUsages.AddAsync(usage, cancellationToken);
    }
}

/// <summary>
/// Repository implementation for Idempotency Record data operations.
/// </summary>
public class IdempotencyRepository : BaseRepository<IdempotencyRecord>, IIdempotencyRepository
{
    /// <summary>
    /// Initializes a new instance of <see cref="IdempotencyRepository"/>.
    /// </summary>
    /// <param name="context">Database context.</param>
    public IdempotencyRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IdempotencyRecord?> GetByKeyAsync(string key, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.IdempotencyRecords
            .FirstOrDefaultAsync(r => r.Key == key && r.UserId == userId, cancellationToken);
    }
}

/// <summary>
/// Repository implementation for Audit Log data operations.
/// </summary>
public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
{
    /// <summary>
    /// Initializes a new instance of <see cref="AuditLogRepository"/>.
    /// </summary>
    /// <param name="context">Database context.</param>
    public AuditLogRepository(ApplicationDbContext context) : base(context)
    {
    }
}
