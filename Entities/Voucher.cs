using System.ComponentModel.DataAnnotations;
using concerts_gate.server.Common.Enums;

namespace concerts_gate.server.Entities;

/// <summary>
/// Entity representing promotional discount vouchers for ticket campaigns.
/// </summary>
/// <remarks>
/// Incorporates full voucher abuse prevention constraints including system-wide limits,
/// per-user redemption limits, minimum order thresholds, discount caps, and optimistic concurrency via <see cref="RowVersion"/>.
/// </remarks>
public class Voucher
{
    /// <summary>
    /// Unique identifier of the voucher (GUID).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Promotional voucher code entered by users (e.g. "FLASHSALE50", "EARLYBIRD10", "VIPCONCERT").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Campaign title or detailed description of the promotional offer.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Discount type (Percentage or FixedAmount).
    /// </summary>
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

    /// <summary>
    /// Discount value (percentage rate or fixed amount in VND).
    /// </summary>
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// Maximum discount amount cap applicable per booking order (when using Percentage discount).
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// Minimum order subtotal required to apply this voucher (VND).
    /// </summary>
    public decimal MinOrderAmount { get; set; } = 0;

    /// <summary>
    /// Total maximum number of allowed redemptions across the system.
    /// </summary>
    public int MaxUsageCount { get; set; }

    /// <summary>
    /// Number of successful redemptions recorded so far.
    /// </summary>
    public int CurrentUsageCount { get; set; } = 0;

    /// <summary>
    /// Maximum number of redemptions permitted per individual user account.
    /// </summary>
    public int MaxUsagePerUser { get; set; } = 1;

    /// <summary>
    /// Validity start timestamp (UTC).
    /// </summary>
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// Validity expiration timestamp (UTC).
    /// </summary>
    public DateTime ValidTo { get; set; }

    /// <summary>
    /// Active status flag for administrative activation/deactivation.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Concurrency token to prevent overselling of limited voucher quotas during simultaneous checkouts.
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Redemptions log associated with this voucher.
    /// </summary>
    public virtual ICollection<VoucherUsage> Usages { get; set; } = new List<VoucherUsage>();
}
