using System.ComponentModel.DataAnnotations;

namespace concerts_gate.server.DTOs.Tickets;

/// <summary>
/// Input payload for creating a new ticket category for a concert.
/// </summary>
public class CreateTicketCategoryDto
{
    /// <summary>
    /// Name of the ticket category (e.g. VIP, Early Bird, General Admission).
    /// </summary>
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tier description and benefits.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Unit ticket price (VND).
    /// </summary>
    [Range(0, 1000000000, ErrorMessage = "Unit ticket price must be greater than or equal to 0.")]
    public decimal Price { get; set; }

    /// <summary>
    /// Total quantity of tickets to issue.
    /// </summary>
    [Range(1, 1000000, ErrorMessage = "Total ticket quantity must be at least 1.")]
    public int TotalQuantity { get; set; }

    /// <summary>
    /// Maximum allowed tickets per order (default is 4).
    /// </summary>
    [Range(1, 20, ErrorMessage = "Max tickets per order must be between 1 and 20.")]
    public int MaxPerOrder { get; set; } = 4;
}
