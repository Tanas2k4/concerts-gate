using System.ComponentModel.DataAnnotations;
using concerts_gate.server.Common.Enums;

namespace concerts_gate.server.DTOs.Operations;

/// <summary>
/// Request payload for manually updating a booking's status by Operator / Admin.
/// </summary>
public class UpdateBookingStatusDto
{
    /// <summary>
    /// New booking status (Confirmed, Cancelled, Suspicious, Refunded).
    /// </summary>
    [Required(ErrorMessage = "New status is required.")]
    public BookingStatus NewStatus { get; set; }

    /// <summary>
    /// Reason or operational justification for the status change.
    /// </summary>
    [Required(ErrorMessage = "Intervention reason is required.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Intervention reason must be between 5 and 500 characters.")]
    public string Reason { get; set; } = string.Empty;
}
