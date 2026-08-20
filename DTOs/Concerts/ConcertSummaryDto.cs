using concerts_gate.server.Common.Enums;

namespace concerts_gate.server.DTOs.Concerts;

/// <summary>
/// Summary concert data displayed in search listings and the catalog homepage.
/// </summary>
public class ConcertSummaryDto
{
    /// <summary>
    /// Concert unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Concert title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Performing artist or headline band.
    /// </summary>
    public string Artist { get; set; } = string.Empty;

    /// <summary>
    /// Venue location.
    /// </summary>
    public string Venue { get; set; } = string.Empty;

    /// <summary>
    /// Music genre.
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// URL to the banner cover image.
    /// </summary>
    public string BannerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Event performance date and time (UTC).
    /// </summary>
    public DateTime EventDate { get; set; }

    /// <summary>
    /// Ticket sales start date and time.
    /// </summary>
    public DateTime SaleStartDate { get; set; }

    /// <summary>
    /// Ticket sales end date and time.
    /// </summary>
    public DateTime SaleEndDate { get; set; }

    /// <summary>
    /// Publication status (Draft, Published, etc.).
    /// </summary>
    public ConcertStatus Status { get; set; }

    /// <summary>
    /// Indicates whether the concert is featured as a Flash Sale.
    /// </summary>
    public bool IsFlashSale { get; set; }

    /// <summary>
    /// Minimum ticket price among all tiers.
    /// </summary>
    public decimal MinPrice { get; set; }

    /// <summary>
    /// Maximum ticket price among all tiers.
    /// </summary>
    public decimal MaxPrice { get; set; }

    /// <summary>
    /// Indicates whether ticket sales are currently open.
    /// </summary>
    public bool IsSaleOpen => DateTime.UtcNow >= SaleStartDate && DateTime.UtcNow <= SaleEndDate && Status == ConcertStatus.Published;

    /// <summary>
    /// Total remaining tickets across all categories.
    /// </summary>
    public int TotalRemainingTickets { get; set; }
}
