using System.ComponentModel.DataAnnotations;

namespace concerts_gate.server.DTOs.Bookings;

/// <summary>
/// Request payload for creating a temporary ticket reservation.
/// </summary>
public class CreateBookingRequestDto
{
    /// <summary>
    /// Identifier of the concert being booked.
    /// </summary>
    [Required(ErrorMessage = "Concert ID is required.")]
    public Guid ConcertId { get; set; }

    /// <summary>
    /// List of ticket categories and requested quantities.
    /// </summary>
    [Required(ErrorMessage = "Booking items list cannot be empty.")]
    [MinLength(1, ErrorMessage = "At least one ticket category must be selected.")]
    public List<BookingItemRequestDto> Items { get; set; } = new List<BookingItemRequestDto>();

    /// <summary>
    /// Promotional voucher code (optional).
    /// </summary>
    public string? VoucherCode { get; set; }
}
