using System.ComponentModel.DataAnnotations;

namespace concerts_gate.server.DTOs.Tickets;

/// <summary>
/// Input payload for adjusting ticket category inventory from the operations dashboard.
/// </summary>
public class UpdateInventoryDto
{
    /// <summary>
    /// New total ticket quantity to set (must not be lower than sold + reserved tickets).
    /// </summary>
    [Range(0, 1000000, ErrorMessage = "Total ticket quantity is invalid.")]
    public int NewTotalQuantity { get; set; }
}
