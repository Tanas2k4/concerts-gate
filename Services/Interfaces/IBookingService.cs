using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Bookings;

namespace concerts_gate.server.Services.Interfaces;

/// <summary>
/// Core business logic engine for ticket booking workflows.
/// </summary>
/// <remarks>
/// Incorporates anti-overselling concurrency control, idempotency tracking against network retries,
/// voucher redemption logic, and electronic ticket issuance upon payment confirmation.
/// </remarks>
public interface IBookingService
{
    /// <summary>
    /// Creates a temporary ticket hold reservation with an expiration TTL (10-15 minutes).
    /// </summary>
    /// <param name="request">Requested ticket categories and quantities.</param>
    /// <param name="userId">Authenticated customer ID.</param>
    /// <param name="idempotencyKey">Client-supplied idempotency key header.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="BookingResponseDto"/> containing reservation details and payment countdown.</returns>
    /// <exception cref="Common.Exceptions.BadRequestException">Thrown when request payload violates business constraints.</exception>
    /// <exception cref="Common.Exceptions.ConcurrencyException">Thrown when tickets are sold out concurrently.</exception>
    Task<BookingResponseDto> CreateBookingAsync(
        CreateBookingRequestDto request,
        Guid userId,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Simulates payment completion and issues electronic tickets with QR codes.
    /// </summary>
    /// <param name="bookingId">Booking order ID.</param>
    /// <param name="userId">Booking owner user ID.</param>
    /// <param name="request">Payment simulation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="BookingResponseDto"/> with Confirmed status and issued e-tickets.</returns>
    Task<BookingResponseDto> ProcessPaymentAsync(
        Guid bookingId,
        Guid userId,
        PaymentSimulationRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a pending booking order and releases reserved tickets back to available inventory.
    /// </summary>
    /// <param name="bookingId">Booking order ID.</param>
    /// <param name="userId">User ID (or null for operator/admin).</param>
    /// <param name="reason">Cancellation reason.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successfully cancelled.</returns>
    Task<bool> CancelBookingAsync(
        Guid bookingId,
        Guid? userId,
        string reason = "Cancelled by customer.",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves booking details by ID.
    /// </summary>
    /// <param name="bookingId">Booking ID.</param>
    /// <param name="userId">User ID for authorization check (null for internal staff).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detailed booking response.</returns>
    Task<BookingResponseDto> GetBookingByIdAsync(Guid bookingId, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves paginated booking history for the current customer.
    /// </summary>
    /// <param name="userId">Customer ID.</param>
    /// <param name="pageIndex">Current page index.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated booking orders.</returns>
    Task<PaginatedResult<BookingResponseDto>> GetMyBookingsAsync(
        Guid userId,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans and releases all expired pending reservations (invoked by background workers).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of expired bookings processed.</returns>
    Task<int> ReleaseExpiredBookingsAsync(CancellationToken cancellationToken = default);
}
