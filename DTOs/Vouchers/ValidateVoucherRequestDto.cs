using System.ComponentModel.DataAnnotations;

namespace concerts_gate.server.DTOs.Vouchers;

/// <summary>
/// Request payload for validating and calculating discount savings for a voucher before checkout.
/// </summary>
public class ValidateVoucherRequestDto
{
    /// <summary>
    /// Promotional voucher code to validate.
    /// </summary>
    [Required(ErrorMessage = "Voucher code is required.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Order subtotal amount before discount (VND).
    /// </summary>
    [Range(1, 1000000000, ErrorMessage = "Order amount must be greater than 0.")]
    public decimal OrderAmount { get; set; }
}
