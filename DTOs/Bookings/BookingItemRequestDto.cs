using System.ComponentModel.DataAnnotations;

namespace concerts_gate.server.DTOs.Bookings;

/// <summary>
/// Line item request for reserving a specific ticket category.
/// </summary>
public class BookingItemRequestDto
{
    /// <summary>
    /// Identifier of the selected ticket category.
    /// </summary>
    [Required(ErrorMessage = "Ticket category ID is required.")]
    public Guid TicketCategoryId { get; set; }

    /// <summary>
    /// Quantity of tickets to reserve for this category.
    /// </summary>
    [Range(1, 10, ErrorMessage = "Ticket quantity per category must be between 1 and 10.")]
    public int Quantity { get; set; }
}
