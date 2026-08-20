namespace concerts_gate.server.DTOs.Operations;

/// <summary>
/// Inventory integrity audit report per concert.
/// </summary>
public class InventoryValidationReportDto
{
    /// <summary>
    /// Concert identifier.
    /// </summary>
    public Guid ConcertId { get; set; }

    /// <summary>
    /// Concert title.
    /// </summary>
    public string ConcertTitle { get; set; } = string.Empty;

    /// <summary>
    /// Category-level inventory reconciliation checks.
    /// </summary>
    public List<CategoryInventoryCheckDto> Categories { get; set; } = new List<CategoryInventoryCheckDto>();

    /// <summary>
    /// Indicates whether all ticket categories are perfectly consistent (Remaining + Reserved + Sold == Total).
    /// </summary>
    public bool IsInventoryConsistent => Categories.All(c => c.IsBalanced);
}
