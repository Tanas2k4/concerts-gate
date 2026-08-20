namespace concerts_gate.server.Entities;

/// <summary>
/// Entity representing the quantity and unit price of a specific ticket category within a booking.
/// </summary>
public class BookingItem
{
    /// <summary>
    /// Unique identifier of the line item.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifier of the parent booking.
    /// </summary>
    public Guid BookingId { get; set; }

    /// <summary>
    /// Identifier of the reserved ticket category.
    /// </summary>
    public Guid TicketCategoryId { get; set; }

    /// <summary>
    /// Quantity of tickets selected for this category.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price at the time of reservation (VND).
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Subtotal for this line item (Quantity * UnitPrice).
    /// </summary>
    public decimal Subtotal => Quantity * UnitPrice;

    /// <summary>
    /// Parent booking entity.
    /// </summary>
    public virtual Booking Booking { get; set; } = null!;

    /// <summary>
    /// Associated ticket category entity.
    /// </summary>
    public virtual TicketCategory TicketCategory { get; set; } = null!;
}
