using concerts_gate.server.DTOs.Tickets;

namespace concerts_gate.server.DTOs.Concerts;

/// <summary>
/// Detailed concert payload including full description and available ticket categories.
/// </summary>
public class ConcertDetailDto : ConcertSummaryDto
{
    /// <summary>
    /// Detailed description of the concert event.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Available ticket categories for booking.
    /// </summary>
    public List<TicketCategoryDto> Categories { get; set; } = new List<TicketCategoryDto>();
}
