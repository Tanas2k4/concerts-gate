using concerts_gate.server.Common.Enums;

namespace concerts_gate.server.Entities;

/// <summary>
/// Entity representing a customer's concert ticket booking order.
/// </summary>
public class Booking
{
    /// <summary>
    /// Unique identifier of the booking order (GUID).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User-friendly booking reference code (e.g. "CG-20260819-8X9Y").
    /// </summary>
    public string BookingCode { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the user who placed the booking.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Identifier of the corresponding concert.
    /// </summary>
    public Guid ConcertId { get; set; }

    /// <summary>
    /// Original total amount before discounts (VND).
    /// </summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>
    /// Discount amount applied via promotional voucher (VND).
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Final amount payable by the customer (VND).
    /// </summary>
    public decimal FinalAmount { get; set; }

    /// <summary>
    /// Current processing status in the booking state machine lifecycle.
    /// </summary>
    public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;

    /// <summary>
    /// Idempotency key associated with this creation request to avoid duplicate orders on network retries.
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Voucher code applied to this booking (if any).
    /// </summary>
    public string? AppliedVoucherCode { get; set; }

    /// <summary>
    /// Expiration timestamp of the temporary ticket hold (TTL 10-15 minutes).
    /// After this timestamp, if unpaid, background workers automatically transition the booking to Expired and release tickets.
    /// </summary>
    public DateTime ReservationExpiresAt { get; set; }

    /// <summary>
    /// Timestamp when payment was completed (UTC).
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Creation timestamp of the booking (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last status update timestamp (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Operational or customer notes (e.g. cancellation reason, fraud investigation details).
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// User who owns this booking.
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Concert being booked.
    /// </summary>
    public virtual Concert Concert { get; set; } = null!;

    /// <summary>
    /// Line items containing ticket category quantities and unit prices.
    /// </summary>
    public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();

    /// <summary>
    /// Electronic tickets issued once payment is confirmed.
    /// </summary>
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
