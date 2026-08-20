using concerts_gate.server.Common.Enums;

namespace concerts_gate.server.DTOs.Bookings;

/// <summary>
/// Comprehensive booking response payload.
/// </summary>
public class BookingResponseDto
{
    /// <summary>
    /// Booking unique identifier (GUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User-friendly booking reference code (e.g. "CG-20260819-8X9Y").
    /// </summary>
    public string BookingCode { get; set; } = string.Empty;

    /// <summary>
    /// Concert identifier.
    /// </summary>
    public Guid ConcertId { get; set; }

    /// <summary>
    /// Concert title.
    /// </summary>
    public string ConcertTitle { get; set; } = string.Empty;

    /// <summary>
    /// Performing artist.
    /// </summary>
    public string Artist { get; set; } = string.Empty;

    /// <summary>
    /// Venue location.
    /// </summary>
    public string Venue { get; set; } = string.Empty;

    /// <summary>
    /// Event date and time.
    /// </summary>
    public DateTime EventDate { get; set; }

    /// <summary>
    /// Original subtotal before discounts (VND).
    /// </summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>
    /// Discount amount applied (VND).
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Final amount payable (VND).
    /// </summary>
    public decimal FinalAmount { get; set; }

    /// <summary>
    /// Current booking status (PendingPayment, Confirmed, Expired, Cancelled, Suspicious, Refunded).
    /// </summary>
    public BookingStatus Status { get; set; }

    /// <summary>
    /// Applied voucher code (if any).
    /// </summary>
    public string? AppliedVoucherCode { get; set; }

    /// <summary>
    /// Reservation expiration timestamp (TTL).
    /// </summary>
    public DateTime ReservationExpiresAt { get; set; }

    /// <summary>
    /// Seconds remaining before reservation expires (countdown).
    /// </summary>
    public int RemainingSeconds => (int)Math.Max(0, (ReservationExpiresAt - DateTime.UtcNow).TotalSeconds);

    /// <summary>
    /// Payment completion timestamp (if paid).
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Booking creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Operational or customer notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Itemized list of ticket categories and quantities in this booking.
    /// </summary>
    public List<BookingItemDto> Items { get; set; } = new List<BookingItemDto>();

    /// <summary>
    /// Electronic tickets issued (populated when status is Confirmed).
    /// </summary>
    public List<TicketDto> Tickets { get; set; } = new List<TicketDto>();
}
