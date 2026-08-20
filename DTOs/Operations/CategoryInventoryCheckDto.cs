namespace concerts_gate.server.DTOs.Operations;

/// <summary>
/// Inventory reconciliation per ticket category (Remaining + Reserved + Sold == Total).
/// </summary>
public class CategoryInventoryCheckDto
{
    /// <summary>
    /// Ticket category ID.
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Ticket category name.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Total original ticket capacity allocated.
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// Available tickets remaining.
    /// </summary>
    public int RemainingQuantity { get; set; }

    /// <summary>
    /// Reserved tickets currently on hold (PendingPayment).
    /// </summary>
    public int ReservedQuantity { get; set; }

    /// <summary>
    /// Confirmed sold tickets.
    /// </summary>
    public int SoldQuantity { get; set; }

    /// <summary>
    /// Calculated total (Remaining + Reserved + Sold).
    /// </summary>
    public int CalculatedTotal => RemainingQuantity + ReservedQuantity + SoldQuantity;

    /// <summary>
    /// Indicates whether the category inventory is perfectly balanced.
    /// </summary>
    public bool IsBalanced => CalculatedTotal == TotalQuantity;
}
