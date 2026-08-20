using System.ComponentModel.DataAnnotations;

namespace concerts_gate.server.Entities;

/// <summary>
/// Entity representing a ticket category / tier in a concert (e.g. VIP, Standard, Early Bird, VVIP).
/// </summary>
/// <remarks>
/// This entity utilizes a <see cref="RowVersion"/> token for Optimistic Concurrency Control (OCC),
/// preventing overselling during high-traffic flash sale events.
/// </remarks>
public class TicketCategory
{
    /// <summary>
    /// Unique identifier of the ticket category.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifier of the concert owning this tier.
    /// </summary>
    public Guid ConcertId { get; set; }

    /// <summary>
    /// Ticket category name (e.g. "VIP Diamond", "Zone A - Standing", "Standard Tier 1").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of tier benefits and perks (e.g. "Includes Welcome Drink + Signed Poster").
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Price per single ticket in this category (VND).
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Total initial quantity of tickets allocated to this category.
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// Remaining quantity available for reservation.
    /// Deducted immediately when a booking order is placed (PendingPayment).
    /// </summary>
    public int RemainingQuantity { get; set; }

    /// <summary>
    /// Quantity of tickets currently on hold (PendingPayment) waiting for customer payment.
    /// </summary>
    public int ReservedQuantity { get; set; }

    /// <summary>
    /// Quantity of tickets successfully purchased and confirmed (Confirmed).
    /// </summary>
    public int SoldQuantity { get; set; }

    /// <summary>
    /// Maximum number of tickets allowed per order for this category.
    /// </summary>
    public int MaxPerOrder { get; set; } = 4;

    /// <summary>
    /// RowVersion concurrency token for Optimistic Concurrency Control.
    /// Automatically managed by SQL Server on record updates.
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Associated concert entity.
    /// </summary>
    public virtual Concert Concert { get; set; } = null!;

    /// <summary>
    /// Booking line items associated with this category.
    /// </summary>
    public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();

    /// <summary>
    /// Electronic tickets issued under this category.
    /// </summary>
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
