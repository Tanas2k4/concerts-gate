namespace concerts_gate.server.Common.Enums;

/// <summary>
/// Discount calculation method for promotional vouchers (Percentage, FixedAmount).
/// </summary>
public enum DiscountType
{
    /// <summary>
    /// Percentage discount applied to the order subtotal (e.g., 10%, 20%).
    /// </summary>
    Percentage = 0,

    /// <summary>
    /// Fixed monetary amount discount (e.g., $5, $10, 50,000 VND).
    /// </summary>
    FixedAmount = 1
}
