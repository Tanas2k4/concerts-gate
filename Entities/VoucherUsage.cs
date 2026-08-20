namespace concerts_gate.server.Entities;

/// <summary>
/// Entity tracking redemption history of vouchers against specific booking orders.
/// </summary>
public class VoucherUsage
{
    /// <summary>
    /// Unique identifier of the usage record (GUID).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifier of the applied voucher.
    /// </summary>
    public Guid VoucherId { get; set; }

    /// <summary>
    /// Identifier of the user who redeemed the voucher.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Identifier of the discounted booking.
    /// </summary>
    public Guid BookingId { get; set; }

    /// <summary>
    /// Actual discount amount deducted from the booking (VND).
    /// </summary>
    public decimal DiscountApplied { get; set; }

    /// <summary>
    /// Timestamp of redemption (UTC).
    /// </summary>
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Associated voucher entity.
    /// </summary>
    public virtual Voucher Voucher { get; set; } = null!;

    /// <summary>
    /// User who applied the voucher.
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Associated booking entity.
    /// </summary>
    public virtual Booking Booking { get; set; } = null!;
}
