using System.ComponentModel.DataAnnotations;

namespace concerts_gate.server.DTOs.Operations;

/// <summary>
/// Request payload for flagging or unflagging a booking as suspicious.
/// </summary>
public class FlagSuspiciousBookingDto
{
    /// <summary>
    /// Mark as suspicious (true) or clear the flag (false).
    /// </summary>
    public bool IsSuspicious { get; set; }

    /// <summary>
    /// Rationale for flagging (e.g. anomalous IP, multiple failed card attempts, rapid bot requests).
    /// </summary>
    [Required(ErrorMessage = "Suspicion reason is required.")]
    public string Reason { get; set; } = string.Empty;
}
