using concerts_gate.server.Common.Enums;
using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Bookings;
using concerts_gate.server.DTOs.Operations;

namespace concerts_gate.server.Services.Interfaces;

/// <summary>
/// Provides operational business logic for the internal operations dashboard and admin workflows.
/// </summary>
public interface IOperationService
{
    /// <summary>
    /// Computes system-wide operational metrics including revenue, ticket counts, and breakdown by booking status.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="DashboardStatsDto"/> stats overview.</returns>
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of bookings across the system for monitoring, filtering, and operational intervention.
    /// </summary>
    /// <param name="status">Filter by booking status.</param>
    /// <param name="concertId">Filter by concert ID.</param>
    /// <param name="search">Search by booking code, user email, user name, or concert title.</param>
    /// <param name="pageIndex">Current page index.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated bookings result.</returns>
    Task<PaginatedResult<BookingResponseDto>> GetBookingsForMonitoringAsync(
        BookingStatus? status = null,
        Guid? concertId = null,
        string? search = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually modifies the status of a booking order by an Operator or Administrator.
    /// </summary>
    /// <param name="bookingId">Booking ID.</param>
    /// <param name="dto">New status and intervention rationale.</param>
    /// <param name="operatorId">Operator ID performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated booking response.</returns>
    Task<BookingResponseDto> UpdateBookingStatusManuallyAsync(
        Guid bookingId,
        UpdateBookingStatusDto dto,
        Guid operatorId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Flags or unflags a booking as suspicious (fraud / bot prevention).
    /// </summary>
    /// <param name="bookingId">Booking ID.</param>
    /// <param name="dto">Flagging decision and rationale.</param>
    /// <param name="operatorId">Operator ID performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated booking response.</returns>
    Task<BookingResponseDto> FlagSuspiciousBookingAsync(
        Guid bookingId,
        FlagSuspiciousBookingDto dto,
        Guid operatorId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Audits and validates the inventory consistency of a concert (Remaining + Reserved + Sold == Total).
    /// </summary>
    /// <param name="concertId">Concert ID to reconcile.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="InventoryValidationReportDto"/> audit report.</returns>
    Task<InventoryValidationReportDto> ValidateConcertInventoryAsync(
        Guid concertId,
        CancellationToken cancellationToken = default);
}
