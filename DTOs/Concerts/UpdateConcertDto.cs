using concerts_gate.server.Common.Enums;

namespace concerts_gate.server.DTOs.Concerts;

/// <summary>
/// Payload for updating concert information, including its publication status.
/// </summary>
public class UpdateConcertDto : CreateConcertDto
{
    /// <summary>
    /// Publication status of the concert (Draft, Published, Archived, Cancelled).
    /// </summary>
    public ConcertStatus Status { get; set; }
}
