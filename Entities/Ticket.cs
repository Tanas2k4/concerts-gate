using concerts_gate.server.Common.Enums;

namespace concerts_gate.server.Entities;

/// <summary>
/// Entity representing an individual electronic ticket issued upon successful payment.
/// </summary>
public class Ticket
{
    /// <summary>
    /// Unique identifier of the ticket (GUID).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifier of the parent booking that generated this ticket.
    /// </summary>
    public Guid BookingId { get; set; }

    /// <summary>
    /// Identifier of the associated ticket category.
    /// </summary>
    public Guid TicketCategoryId { get; set; }

    /// <summary>
    /// Unique ticket code used for display and verification (e.g. "TKT-2026-VIP-99482").
    /// </summary>
    public string TicketCode { get; set; } = string.Empty;

    /// <summary>
    /// Encrypted or signed QR code payload scanned at entry gates.
    /// </summary>
    public string QrCodePayload { get; set; } = string.Empty;

    /// <summary>
    /// Validation status of the ticket (Valid, Used, Revoked).
    /// </summary>
    public TicketStatus Status { get; set; } = TicketStatus.Valid;

    /// <summary>
    /// Issuance timestamp (UTC).
    /// </summary>
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gate check-in timestamp (UTC) if scanned.
    /// </summary>
    public DateTime? CheckedInAt { get; set; }

    /// <summary>
    /// Parent booking entity.
    /// </summary>
    public virtual Booking Booking { get; set; } = null!;

    /// <summary>
    /// Associated ticket category entity.
    /// </summary>
    public virtual TicketCategory TicketCategory { get; set; } = null!;
}
