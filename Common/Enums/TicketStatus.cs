namespace concerts_gate.server.Common.Enums;

/// <summary>
/// Status of an individual ticket post-payment (Valid, Used, Revoked).
/// </summary>
public enum TicketStatus
{
    /// <summary>
    /// Ticket is active and valid for admission.
    /// </summary>
    Valid = 0,

    /// <summary>
    /// Ticket has been scanned and admitted at the gate.
    /// </summary>
    Used = 1,

    /// <summary>
    /// Ticket has been revoked or invalidated.
    /// </summary>
    Revoked = 2
}
