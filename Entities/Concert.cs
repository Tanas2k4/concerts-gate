using concerts_gate.server.Common.Enums;

namespace concerts_gate.server.Entities;

/// <summary>
/// Entity representing a music concert or live entertainment event.
/// </summary>
public class Concert
{
    /// <summary>
    /// Unique identifier of the concert (GUID).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Title or name of the concert.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Performing artist or headline band.
    /// </summary>
    public string Artist { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the event.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Venue location (stadium, theater, exhibition center).
    /// </summary>
    public string Venue { get; set; } = string.Empty;

    /// <summary>
    /// URL to the event banner image.
    /// </summary>
    public string BannerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Music genre (Pop, Rock, EDM, Classical, etc.).
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// Scheduled start date and time of the concert (UTC).
    /// </summary>
    public DateTime EventDate { get; set; }

    /// <summary>
    /// Start date and time when ticket sales open (Flash Sale start if applicable).
    /// </summary>
    public DateTime SaleStartDate { get; set; }

    /// <summary>
    /// End date and time when ticket sales close.
    /// </summary>
    public DateTime SaleEndDate { get; set; }

    /// <summary>
    /// Publication status of the concert (Draft, Published, Archived, Cancelled).
    /// </summary>
    public ConcertStatus Status { get; set; } = ConcertStatus.Draft;

    /// <summary>
    /// Indicates whether this concert is part of a high-concurrency Flash Sale campaign.
    /// </summary>
    public bool IsFlashSale { get; set; } = false;

    /// <summary>
    /// Record creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Record last update timestamp (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Ticket categories (VIP, Standard, GA, etc.) available for this concert.
    /// </summary>
    public virtual ICollection<TicketCategory> TicketCategories { get; set; } = new List<TicketCategory>();

    /// <summary>
    /// Booking orders placed for this concert.
    /// </summary>
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
