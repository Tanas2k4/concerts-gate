using concerts_gate.server.Common.Enums;
using concerts_gate.server.Entities;

namespace concerts_gate.server.Repositories.Interfaces;

/// <summary>
/// Repository interface for operations relating to Bookings and individual Tickets.
/// </summary>
public interface IBookingRepository : IBaseRepository<Booking>
{
    /// <summary>
    /// Retrieves a booking with all its related entities (BookingItems, TicketCategory, Concert, Tickets, User).
    /// </summary>
    /// <param name="id">Booking unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A detailed <see cref="Booking"/> instance or null.</returns>
    Task<Booking?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a booking by its friendly booking code (e.g. "CG-20260819-8X9Y").
    /// </summary>
    /// <param name="bookingCode">Booking code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Booking"/> instance or null.</returns>
    Task<Booking?> GetByCodeAsync(string bookingCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves pending payment bookings that have exceeded their reservation hold TTL (ReservationExpiresAt &lt; UTC Now).
    /// Used by background workers to release expired inventory.
    /// </summary>
    /// <param name="now">Current timestamp (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of expired pending bookings.</returns>
    Task<List<Booking>> GetExpiredPendingBookingsAsync(DateTime now, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a collection of issued electronic tickets to the DbContext.
    /// </summary>
    /// <param name="tickets">List of electronic tickets.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddTicketsAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Voucher and VoucherUsage operations.
/// </summary>
public interface IVoucherRepository : IBaseRepository<Voucher>
{
    /// <summary>
    /// Retrieves a voucher by its uppercase code (e.g. "FLASHSALE20").
    /// </summary>
    /// <param name="code">Voucher code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The <see cref="Voucher"/> entity if found, otherwise null.</returns>
    Task<Voucher?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts how many times a user has redeemed a specific voucher.
    /// </summary>
    /// <param name="voucherId">Voucher unique identifier.</param>
    /// <param name="userId">User unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of previous redemptions.</returns>
    Task<int> GetUserUsageCountAsync(Guid voucherId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a new voucher redemption in the <see cref="VoucherUsage"/> table.
    /// </summary>
    /// <param name="usage">Voucher usage record.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddUsageAsync(VoucherUsage usage, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Idempotency records caching.
/// </summary>
public interface IIdempotencyRepository : IBaseRepository<IdempotencyRecord>
{
    /// <summary>
    /// Finds an idempotency record by key and user ID.
    /// </summary>
    /// <param name="key">Idempotency key.</param>
    /// <param name="userId">User unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The existing <see cref="IdempotencyRecord"/> or null.</returns>
    Task<IdempotencyRecord?> GetByKeyAsync(string key, Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Operational and Administrative Audit Trails.
/// </summary>
public interface IAuditLogRepository : IBaseRepository<AuditLog>
{
}
