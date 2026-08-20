namespace concerts_gate.server.DTOs.Operations;

/// <summary>
/// High-level business performance metrics and ticketing status across the entire system for the operations dashboard.
/// </summary>
public class DashboardStatsDto
{
    /// <summary>
    /// Total revenue from successfully completed orders (VND).
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Total count of tickets successfully sold (Confirmed).
    /// </summary>
    public int TotalTicketsSold { get; set; }

    /// <summary>
    /// Total count of tickets currently reserved (PendingPayment) across all concerts.
    /// </summary>
    public int TotalTicketsReserved { get; set; }

    /// <summary>
    /// Total number of bookings created across the system.
    /// </summary>
    public int TotalBookingsCount { get; set; }

    /// <summary>
    /// Number of bookings currently awaiting payment.
    /// </summary>
    public int PendingBookingsCount { get; set; }

    /// <summary>
    /// Number of bookings with confirmed payment.
    /// </summary>
    public int ConfirmedBookingsCount { get; set; }

    /// <summary>
    /// Number of bookings that expired due to unpaid TTL.
    /// </summary>
    public int ExpiredBookingsCount { get; set; }

    /// <summary>
    /// Number of bookings flagged as suspicious (fraud / bot activity).
    /// </summary>
    public int SuspiciousBookingsCount { get; set; }

    /// <summary>
    /// Number of cancelled bookings.
    /// </summary>
    public int CancelledBookingsCount { get; set; }

    /// <summary>
    /// Payment conversion rate (%).
    /// </summary>
    public double ConversionRate => TotalBookingsCount > 0
        ? Math.Round((double)ConfirmedBookingsCount / TotalBookingsCount * 100, 2)
        : 0;

    /// <summary>
    /// Top 5 performing concerts by revenue or sales volume.
    /// </summary>
    public List<TopConcertStatsDto> TopConcerts { get; set; } = new List<TopConcertStatsDto>();
}
