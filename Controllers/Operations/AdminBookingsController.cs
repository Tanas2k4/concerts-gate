using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using concerts_gate.server.Common.Constants;
using concerts_gate.server.Common.Enums;
using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Bookings;
using concerts_gate.server.DTOs.Operations;
using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.Controllers.Operations;

/// <summary>
/// Provides operational monitoring and manual intervention APIs for bookings to Operators and Administrators.
/// </summary>
[ApiController]
[Route("api/admin/bookings")]
[Authorize(Roles = AppConstants.Roles.InternalOperations)]
[Produces("application/json")]
public class AdminBookingsController : ControllerBase
{
    private readonly IOperationService _operationService;
    private readonly IBookingService _bookingService;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminBookingsController"/>.
    /// </summary>
    public AdminBookingsController(IOperationService operationService, IBookingService bookingService)
    {
        _operationService = operationService;
        _bookingService = bookingService;
    }

    /// <summary>
    /// Retrieves a paginated list of all bookings across the system for monitoring (supports status and concert filters, and search).
    /// </summary>
    /// <param name="status">Filter by booking status (PendingPayment, Confirmed, Expired, Suspicious, etc.).</param>
    /// <param name="concertId">Filter by concert ID.</param>
    /// <param name="search">Search by booking code, user email, or customer name.</param>
    /// <param name="pageIndex">Page index (default 1).</param>
    /// <param name="pageSize">Items per page (default 20).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns paginated booking list.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<BookingResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookings(
        [FromQuery] BookingStatus? status,
        [FromQuery] Guid? concertId,
        [FromQuery] string? search,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _operationService.GetBookingsForMonitoringAsync(status, concertId, search, pageIndex, pageSize, cancellationToken);
        return Ok(ApiResponse<PaginatedResult<BookingResponseDto>>.Ok(result));
    }

    /// <summary>
    /// Retrieves full details for any booking order by ID.
    /// </summary>
    /// <param name="id">Booking unique identifier (GUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns detailed booking info.</response>
    /// <response code="404">Booking not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BookingResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookingDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetBookingByIdAsync(id, null, cancellationToken);
        return Ok(ApiResponse<BookingResponseDto>.Ok(result));
    }

    /// <summary>
    /// Manually updates the status of a booking during incident resolution (e.g. manual confirmation, cancellation, refund).
    /// </summary>
    /// <param name="id">Booking unique identifier (GUID).</param>
    /// <param name="dto">New status and intervention rationale.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Status updated and audit log recorded.</response>
    /// <response code="400">Invalid new status.</response>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<BookingResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBookingStatus(
        Guid id,
        [FromBody] UpdateBookingStatusDto dto,
        CancellationToken cancellationToken)
    {
        var operatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _operationService.UpdateBookingStatusManuallyAsync(id, dto, operatorId, cancellationToken);
        return Ok(ApiResponse<BookingResponseDto>.Ok(result, "Booking status updated successfully!"));
    }

    /// <summary>
    /// Flags a booking as suspicious (fraud / bot detection) or clears a previous flag.
    /// </summary>
    /// <param name="id">Booking unique identifier (GUID).</param>
    /// <param name="dto">Suspicion flag and justification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Suspicion status updated.</response>
    [HttpPost("{id:guid}/flag-suspicious")]
    [ProducesResponseType(typeof(ApiResponse<BookingResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> FlagSuspicious(
        Guid id,
        [FromBody] FlagSuspiciousBookingDto dto,
        CancellationToken cancellationToken)
    {
        var operatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _operationService.FlagSuspiciousBookingAsync(id, dto, operatorId, cancellationToken);
        return Ok(ApiResponse<BookingResponseDto>.Ok(result, dto.IsSuspicious ? "Booking flagged as suspicious." : "Suspicion flag cleared."));
    }
}
