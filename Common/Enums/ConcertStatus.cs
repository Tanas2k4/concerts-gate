namespace concerts_gate.server.Common.Enums;

/// <summary>
/// Publication status of a concert (Draft, Published, Archived, Cancelled).
/// </summary>
public enum ConcertStatus
{
    /// <summary>
    /// Draft version, not visible to the public.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Published and visible to the public; tickets can be viewed and booked when sales start.
    /// </summary>
    Published = 1,

    /// <summary>
    /// Concluded or archived.
    /// </summary>
    Archived = 2,

    /// <summary>
    /// Postponed or cancelled.
    /// </summary>
    Cancelled = 3
}
