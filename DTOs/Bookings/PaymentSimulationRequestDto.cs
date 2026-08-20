using System.ComponentModel.DataAnnotations;

namespace concerts_gate.server.DTOs.Bookings;

/// <summary>
/// Request payload for simulating online payment for a booking order.
/// </summary>
public class PaymentSimulationRequestDto
{
    /// <summary>
    /// Payment method (e.g. "VNPAY", "MOMO", "CREDIT_CARD", "GATE_WALLET").
    /// </summary>
    [Required(ErrorMessage = "Payment method is required.")]
    public string PaymentMethod { get; set; } = "VNPAY";

    /// <summary>
    /// Simulated transaction reference code from payment provider.
    /// </summary>
    public string? TransactionReference { get; set; }
}
