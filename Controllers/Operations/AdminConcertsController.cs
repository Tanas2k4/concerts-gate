using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using concerts_gate.server.Common.Constants;
using concerts_gate.server.Common.Enums;
using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Concerts;
using concerts_gate.server.DTOs.Tickets;
using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.Controllers.Operations;

/// <summary>
/// Provides administrative APIs for creating, modifying, publishing concerts, and managing ticket category inventory.
/// </summary>
[ApiController]
[Route("api/admin/concerts")]
[Authorize(Roles = AppConstants.Roles.InternalOperations)]
[Produces("application/json")]
public class AdminConcertsController : ControllerBase
{
    private readonly IConcertService _concertService;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminConcertsController"/>.
    /// </summary>
    public AdminConcertsController(IConcertService concertService)
    {
        _concertService = concertService;
    }

    /// <summary>
    /// Retrieves a paginated list of all concerts (including Draft, Published, Archived) for the operations console.
    /// </summary>
    /// <param name="pageIndex">Page index (default 1).</param>
    /// <param name="pageSize">Items per page (default 20).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns paginated concert list.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ConcertDetailDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllConcerts(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _concertService.GetAllConcertsForAdminAsync(pageIndex, pageSize, cancellationToken);
        return Ok(ApiResponse<PaginatedResult<ConcertDetailDto>>.Ok(result));
    }

    /// <summary>
    /// Creates a new concert event and optional initial ticket categories.
    /// </summary>
    /// <param name="dto">Concert creation payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Concert created successfully.</response>
    /// <response code="400">Invalid payload.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ConcertDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateConcert([FromBody] CreateConcertDto dto, CancellationToken cancellationToken)
    {
        var operatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _concertService.CreateConcertAsync(dto, operatorId, cancellationToken);
        return Ok(ApiResponse<ConcertDetailDto>.Ok(result, "New concert created successfully!"));
    }

    /// <summary>
    /// Updates details of an existing concert.
    /// </summary>
    /// <param name="id">Concert unique identifier (GUID).</param>
    /// <param name="dto">Update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Concert updated successfully.</response>
    /// <response code="404">Concert not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ConcertDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateConcert(Guid id, [FromBody] UpdateConcertDto dto, CancellationToken cancellationToken)
    {
        var operatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _concertService.UpdateConcertAsync(id, dto, operatorId, cancellationToken);
        return Ok(ApiResponse<ConcertDetailDto>.Ok(result, "Concert information updated successfully!"));
    }

    /// <summary>
    /// Changes the publication status of a concert (Draft, Published, Archived, Cancelled).
    /// </summary>
    /// <param name="id">Concert unique identifier.</param>
    /// <param name="status">New status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Status changed successfully.</response>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromQuery] ConcertStatus status, CancellationToken cancellationToken)
    {
        var operatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _concertService.ChangeConcertStatusAsync(id, status, operatorId, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, $"Concert status updated to: {status}"));
    }

    /// <summary>
    /// Adds a new ticket category (VIP, Standard, etc.) to an existing concert.
    /// </summary>
    /// <param name="id">Concert unique identifier.</param>
    /// <param name="dto">Ticket category creation payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Ticket category added successfully.</response>
    [HttpPost("{id:guid}/categories")]
    [ProducesResponseType(typeof(ApiResponse<TicketCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddTicketCategory(Guid id, [FromBody] CreateTicketCategoryDto dto, CancellationToken cancellationToken)
    {
        var operatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _concertService.AddTicketCategoryAsync(id, dto, operatorId, cancellationToken);
        return Ok(ApiResponse<TicketCategoryDto>.Ok(result, "New ticket category added successfully!"));
    }

    /// <summary>
    /// Adjusts the total allocated ticket inventory for a category from the operations dashboard.
    /// </summary>
    /// <param name="categoryId">Ticket category unique identifier (GUID).</param>
    /// <param name="dto">New total inventory quantity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Inventory updated successfully.</response>
    /// <response code="400">New total is lower than sold and reserved quantities combined.</response>
    [HttpPut("categories/{categoryId:guid}/inventory")]
    [ProducesResponseType(typeof(ApiResponse<TicketCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateInventory(Guid categoryId, [FromBody] UpdateInventoryDto dto, CancellationToken cancellationToken)
    {
        var operatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _concertService.UpdateCategoryInventoryAsync(categoryId, dto, operatorId, cancellationToken);
        return Ok(ApiResponse<TicketCategoryDto>.Ok(result, "Ticket category inventory updated successfully!"));
    }
}
