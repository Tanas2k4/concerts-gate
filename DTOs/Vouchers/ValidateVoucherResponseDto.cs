namespace concerts_gate.server.DTOs.Vouchers;

/// <summary>
/// Response payload with validation results and calculated discount amounts.
/// </summary>
public class ValidateVoucherResponseDto
{
    /// <summary>
    /// Validated voucher code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Promotional offer description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Original order subtotal before discount (VND).
    /// </summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>
    /// Discount amount saved (VND).
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Final order total payable after applying discount (VND).
    /// </summary>
    public decimal FinalAmount { get; set; }
}
