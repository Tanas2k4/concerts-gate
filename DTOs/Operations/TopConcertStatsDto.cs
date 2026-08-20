namespace concerts_gate.server.DTOs.Operations;

/// <summary>
/// Detailed sales and occupancy metrics for an individual concert.
/// </summary>
public class TopConcertStatsDto
{
    /// <summary>
    /// Concert identifier.
    /// </summary>
    public Guid ConcertId { get; set; }

    /// <summary>
    /// Concert title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Number of tickets sold.
    /// </summary>
    public int SoldCount { get; set; }

    /// <summary>
    /// Total ticket capacity.
    /// </summary>
    public int TotalCapacity { get; set; }

    /// <summary>
    /// Total revenue generated (VND).
    /// </summary>
    public decimal Revenue { get; set; }

    /// <summary>
    /// Venue occupancy rate (%).
    /// </summary>
    public double OccupancyRate => TotalCapacity > 0 ? Math.Round((double)SoldCount / TotalCapacity * 100, 2) : 0;
}
