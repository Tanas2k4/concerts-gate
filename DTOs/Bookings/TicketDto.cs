using concerts_gate.server.Common.Enums;

namespace concerts_gate.server.DTOs.Bookings;

/// <summary>
/// Electronic ticket details including QR code payload for gate admission.
/// </summary>
public class TicketDto
{
    /// <summary>
    /// Ticket unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Display ticket code (e.g. "TKT-2026-VIP-99482").
    /// </summary>
    public string TicketCode { get; set; } = string.Empty;

    /// <summary>
    /// QR code payload string for scanning at gate check-in.
    /// </summary>
    public string QrCodePayload { get; set; } = string.Empty;

    /// <summary>
    /// Ticket category name.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Ticket status (Valid, Used, Revoked).
    /// </summary>
    public TicketStatus Status { get; set; }

    /// <summary>
    /// Issuance timestamp.
    /// </summary>
    public DateTime IssuedAt { get; set; }
}
