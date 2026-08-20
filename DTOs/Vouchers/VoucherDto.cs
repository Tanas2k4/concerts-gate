using concerts_gate.server.Common.Enums;

namespace concerts_gate.server.DTOs.Vouchers;

/// <summary>
/// Detailed voucher payload for customers and administrators.
/// </summary>
public class VoucherDto
{
    /// <summary>
    /// Voucher unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Voucher code (e.g. "FLASHSALE20").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Promotional campaign description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Discount calculation type (Percentage or FixedAmount).
    /// </summary>
    public DiscountType DiscountType { get; set; }

    /// <summary>
    /// Discount value (% or VND amount).
    /// </summary>
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// Maximum discount cap.
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// Minimum order subtotal required.
    /// </summary>
    public decimal MinOrderAmount { get; set; }

    /// <summary>
    /// Total maximum usage count across the system.
    /// </summary>
    public int MaxUsageCount { get; set; }

    /// <summary>
    /// Actual number of times redeemed so far.
    /// </summary>
    public int CurrentUsageCount { get; set; }

    /// <summary>
    /// Maximum redemptions allowed per user account.
    /// </summary>
    public int MaxUsagePerUser { get; set; }

    /// <summary>
    /// Validity start date and time (UTC).
    /// </summary>
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// Validity expiration date and time (UTC).
    /// </summary>
    public DateTime ValidTo { get; set; }

    /// <summary>
    /// Active status flag.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates whether the voucher is currently active, has remaining quota, and is within valid dates.
    /// </summary>
    public bool IsAvailable => IsActive && CurrentUsageCount < MaxUsageCount && DateTime.UtcNow >= ValidFrom && DateTime.UtcNow <= ValidTo;
}
