using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using concerts_gate.server.Common.Constants;
using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Bookings;
using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.Controllers;

/// <summary>
/// Provides customer-facing booking APIs (Hold reservation, Payment simulation, Cancellation, Booking lookups).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    /// <summary>
    /// Initializes a new instance of <see cref="BookingsController"/>.
    /// </summary>
    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Creates a temporary ticket reservation held for 10 minutes (Reserve Tickets).
    /// </summary>
    /// <remarks>
    /// Supports the <c>X-Idempotency-Key</c> header to prevent duplicate orders upon network retries.
    /// Employs Optimistic Concurrency Control to eliminate overselling in high-traffic Flash Sales.
    /// </remarks>
    /// <param name="request">Concert ID, ticket categories, quantities, and optional voucher code.</param>
    /// <param name="idempotencyKey">Idempotency key from request header (optional but recommended).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Reservation successful, returns booking details and expiration countdown (TTL).</response>
    /// <response code="400">Invalid payload or event not open for sale.</response>
    /// <response code="409">Sold out or concurrency conflict during Flash Sale rush.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BookingResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateBooking(
        [FromBody] CreateBookingRequestDto request,
        [FromHeader(Name = AppConstants.BookingConfig.IdempotencyHeaderName)] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _bookingService.CreateBookingAsync(request, userId, idempotencyKey, cancellationToken);
        return Ok(ApiResponse<BookingResponseDto>.Ok(result, "Ticket reservation successful. Please complete payment before expiration!"));
    }

    /// <summary>
    /// Simulates online payment for a booking order and issues electronic tickets with QR codes.
    /// </summary>
    /// <param name="id">Booking unique identifier (GUID).</param>
    /// <param name="request">Payment method and transaction reference.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Payment completed, status updated to Confirmed and e-tickets returned.</response>
    /// <response code="400">Invalid booking, expired reservation hold, or already completed.</response>
    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(typeof(ApiResponse<BookingResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessPayment(
        Guid id,
        [FromBody] PaymentSimulationRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _bookingService.ProcessPaymentAsync(id, userId, request, cancellationToken);
        return Ok(ApiResponse<BookingResponseDto>.Ok(result, "Payment successful! Your e-tickets are ready."));
    }

    /// <summary>
    /// Customer initiates cancellation of an unpaid reservation hold (PendingPayment) to release tickets back to inventory.
    /// </summary>
    /// <param name="id">Booking unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Reservation cancelled successfully.</response>
    /// <response code="400">Booking is not in a cancellable status.</response>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelBooking(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _bookingService.CancelBookingAsync(id, userId, "Cancelled by customer.", cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Booking cancelled successfully."));
    }

    /// <summary>
    /// Retrieves detailed booking information by ID.
    /// </summary>
    /// <param name="id">Booking unique identifier (GUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns booking details.</response>
    /// <response code="404">Booking not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BookingResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookingById(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _bookingService.GetBookingByIdAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<BookingResponseDto>.Ok(result));
    }

    /// <summary>
    /// Retrieves paginated booking history for the current customer (includes status, amounts, and QR tickets).
    /// </summary>
    /// <param name="pageIndex">Page index (default 1).</param>
    /// <param name="pageSize">Items per page (default 10).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns paginated booking list.</response>
    [HttpGet("my-bookings")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<BookingResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyBookings(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _bookingService.GetMyBookingsAsync(userId, pageIndex, pageSize, cancellationToken);
        return Ok(ApiResponse<PaginatedResult<BookingResponseDto>>.Ok(result));
    }
}
