namespace concerts_gate.server.Entities;

/// <summary>
/// Entity recording operational and administrative audit trails.
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Unique identifier of the audit log record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifier of the operator or administrator performing the action.
    /// </summary>
    public Guid OperatorId { get; set; }

    /// <summary>
    /// Name or email of the person who executed the action.
    /// </summary>
    public string OperatorName { get; set; } = string.Empty;

    /// <summary>
    /// Action performed (e.g. "UPDATE_BOOKING_STATUS", "FLAG_SUSPICIOUS", "PUBLISH_CONCERT", "UPDATE_INVENTORY").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Name of the affected entity/table (e.g. "Booking", "Concert", "TicketCategory", "Voucher").
    /// </summary>
    public string TargetEntity { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the target entity instance.
    /// </summary>
    public string TargetId { get; set; } = string.Empty;

    /// <summary>
    /// Details, state diff, or operational rationale recorded as text or JSON.
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the action occurred (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
