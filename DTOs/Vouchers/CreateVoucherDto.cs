using System.ComponentModel.DataAnnotations;
using concerts_gate.server.Common.Enums;

namespace concerts_gate.server.DTOs.Vouchers;

/// <summary>
/// Input payload for creating a new promotional voucher campaign (Admin/Operator).
/// </summary>
public class CreateVoucherDto
{
    /// <summary>
    /// Promotional voucher code in uppercase alphanumeric characters (e.g. "VIP2026").
    /// </summary>
    [Required(ErrorMessage = "Voucher code is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Voucher code must be between 3 and 50 characters.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Campaign description and offer details.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Discount calculation type (Percentage or FixedAmount).
    /// </summary>
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

    /// <summary>
    /// Discount amount (% between 1-100 if Percentage, or fixed VND amount > 0).
    /// </summary>
    [Range(1, 1000000000, ErrorMessage = "Discount value is invalid.")]
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// Maximum discount cap when using Percentage discount.
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// Minimum order subtotal required to apply voucher (VND).
    /// </summary>
    public decimal MinOrderAmount { get; set; } = 0;

    /// <summary>
    /// Total maximum usage count across the entire system.
    /// </summary>
    [Range(1, 1000000, ErrorMessage = "Max usage count must be at least 1.")]
    public int MaxUsageCount { get; set; } = 100;

    /// <summary>
    /// Maximum redemptions allowed per customer account (default is 1).
    /// </summary>
    [Range(1, 100, ErrorMessage = "Per-user limit must be at least 1.")]
    public int MaxUsagePerUser { get; set; } = 1;

    /// <summary>
    /// Validity start date and time (UTC).
    /// </summary>
    [Required]
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// Validity expiration date and time (UTC).
    /// </summary>
    [Required]
    public DateTime ValidTo { get; set; }

    /// <summary>
    /// Active status flag upon creation.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
