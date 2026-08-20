using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using concerts_gate.server.Common.Constants;
using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Operations;
using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.Controllers.Operations;

/// <summary>
/// Provides operational dashboard overview statistics and inventory validation APIs for internal operations.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = AppConstants.Roles.InternalOperations)]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IOperationService _operationService;

    /// <summary>
    /// Initializes a new instance of <see cref="DashboardController"/>.
    /// </summary>
    public DashboardController(IOperationService operationService)
    {
        _operationService = operationService;
    }

    /// <summary>
    /// Retrieves full operational summary metrics: Revenue, Sold tickets, Reserved tickets, Breakdown by status, and Top concerts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns dashboard metrics.</response>
    /// <response code="403">Forbidden from accessing internal operations dashboard.</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        var result = await _operationService.GetDashboardStatsAsync(cancellationToken);
        return Ok(ApiResponse<DashboardStatsDto>.Ok(result));
    }

    /// <summary>
    /// Checks and audits the inventory consistency (Remaining + Reserved + Sold == Total) for a specific concert event.
    /// </summary>
    /// <param name="concertId">Concert unique identifier to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns inventory validation report.</response>
    /// <response code="404">Concert not found.</response>
    [HttpGet("inventory-validation/{concertId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryValidationReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateInventory(Guid concertId, CancellationToken cancellationToken)
    {
        var result = await _operationService.ValidateConcertInventoryAsync(concertId, cancellationToken);
        return Ok(ApiResponse<InventoryValidationReportDto>.Ok(result, "Ticket inventory validation completed!"));
    }
}
