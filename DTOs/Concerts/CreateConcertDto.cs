using System.ComponentModel.DataAnnotations;
using concerts_gate.server.DTOs.Tickets;

namespace concerts_gate.server.DTOs.Concerts;

/// <summary>
/// Input payload for creating a new concert (Admin/Operator).
/// </summary>
public class CreateConcertDto
{
    /// <summary>
    /// Title of the concert.
    /// </summary>
    [Required(ErrorMessage = "Concert title is required.")]
    [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Main performing artist or band name.
    /// </summary>
    [Required(ErrorMessage = "Artist name is required.")]
    [StringLength(200, ErrorMessage = "Artist name cannot exceed 200 characters.")]
    public string Artist { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the event.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Venue location.
    /// </summary>
    [Required(ErrorMessage = "Venue is required.")]
    [StringLength(300, ErrorMessage = "Venue cannot exceed 300 characters.")]
    public string Venue { get; set; } = string.Empty;

    /// <summary>
    /// URL to the banner cover image.
    /// </summary>
    public string BannerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Music genre.
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// Event performance date and time.
    /// </summary>
    [Required(ErrorMessage = "Event date is required.")]
    public DateTime EventDate { get; set; }

    /// <summary>
    /// Ticket sales start date and time.
    /// </summary>
    [Required(ErrorMessage = "Sale start date is required.")]
    public DateTime SaleStartDate { get; set; }

    /// <summary>
    /// Ticket sales end date and time.
    /// </summary>
    [Required(ErrorMessage = "Sale end date is required.")]
    public DateTime SaleEndDate { get; set; }

    /// <summary>
    /// Indicates whether this concert is part of a Flash Sale campaign.
    /// </summary>
    public bool IsFlashSale { get; set; } = false;

    /// <summary>
    /// Optional ticket categories initialized along with the concert.
    /// </summary>
    public List<CreateTicketCategoryDto>? Categories { get; set; }
}
