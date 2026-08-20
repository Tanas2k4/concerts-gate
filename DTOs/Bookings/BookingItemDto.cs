namespace concerts_gate.server.DTOs.Bookings;

/// <summary>
/// Booking item details returned to the customer.
/// </summary>
public class BookingItemDto
{
    /// <summary>
    /// Line item unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Ticket category identifier.
    /// </summary>
    public Guid TicketCategoryId { get; set; }

    /// <summary>
    /// Ticket category name.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Ticket quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price at time of booking.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Line subtotal.
    /// </summary>
    public decimal Subtotal => Quantity * UnitPrice;
}
