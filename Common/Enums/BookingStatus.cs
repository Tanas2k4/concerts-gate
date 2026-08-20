namespace concerts_gate.server.Common.Enums;

/// <summary>
/// Status of a booking order across its state machine lifecycle (PendingPayment, Confirmed, Expired, Cancelled, Suspicious, Refunded).
/// </summary>
public enum BookingStatus
{
    /// <summary>
    /// Temporarily reserved, waiting for customer payment within the expiration TTL (10-15 minutes).
    /// </summary>
    PendingPayment = 0,

    /// <summary>
    /// Payment successful, e-tickets (QR code / ticket codes) issued.
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Payment TTL expired without completion; system automatically cancels the order and releases reserved tickets back to inventory.
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Booking cancelled by the customer or operations staff.
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Booking flagged as suspicious (potential bot or fraudulent activity) requiring manual review.
    /// </summary>
    Suspicious = 4,

    /// <summary>
    /// Booking has been refunded to the customer.
    /// </summary>
    Refunded = 5
}
