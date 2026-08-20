namespace concerts_gate.server.DTOs.Tickets;

/// <summary>
/// Detailed ticket category payload returned to clients and the operations dashboard.
/// </summary>
public class TicketCategoryDto
{
    /// <summary>
    /// Ticket category unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Associated concert identifier.
    /// </summary>
    public Guid ConcertId { get; set; }

    /// <summary>
    /// Category name (e.g. VIP, Standard, GA).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of category benefits.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Unit ticket price (VND).
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Initial total quantity of tickets allocated.
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// Available tickets remaining for reservation.
    /// </summary>
    public int RemainingQuantity { get; set; }

    /// <summary>
    /// Tickets currently on hold (PendingPayment).
    /// </summary>
    public int ReservedQuantity { get; set; }

    /// <summary>
    /// Tickets successfully purchased and confirmed (Confirmed).
    /// </summary>
    public int SoldQuantity { get; set; }

    /// <summary>
    /// Maximum allowed tickets per booking order.
    /// </summary>
    public int MaxPerOrder { get; set; }

    /// <summary>
    /// Indicates whether this category is completely sold out.
    /// </summary>
    public bool IsSoldOut => RemainingQuantity <= 0;
}
