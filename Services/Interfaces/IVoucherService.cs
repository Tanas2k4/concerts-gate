using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Vouchers;
using concerts_gate.server.Entities;

namespace concerts_gate.server.Services.Interfaces;

/// <summary>
/// Provides business methods for promotional voucher management, validation, and abuse prevention.
/// </summary>
public interface IVoucherService
{
    /// <summary>
    /// Validates a voucher code for a user and calculates applicable discounts against an order amount.
    /// </summary>
    /// <param name="code">Voucher code.</param>
    /// <param name="userId">User ID.</param>
    /// <param name="orderAmount">Order subtotal amount before discount (VND).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="ValidateVoucherResponseDto"/> with calculated discount amounts.</returns>
    Task<ValidateVoucherResponseDto> ValidateAndCalculateDiscountAsync(
        string code,
        Guid userId,
        decimal orderAmount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of all promotional vouchers for administrative management.
    /// </summary>
    /// <param name="pageIndex">Current page index.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated vouchers list.</returns>
    Task<PaginatedResult<VoucherDto>> GetAllVouchersAsync(int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new promotional voucher campaign.
    /// </summary>
    /// <param name="dto">Voucher configuration parameters.</param>
    /// <param name="operatorId">Operator ID creating the voucher.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created voucher details.</returns>
    Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto dto, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Toggles the active status of a voucher.
    /// </summary>
    /// <param name="id">Voucher ID.</param>
    /// <param name="isActive">New active status.</param>
    /// <param name="operatorId">Operator ID performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successfully toggled.</returns>
    Task<bool> ToggleVoucherStatusAsync(Guid id, bool isActive, Guid operatorId, CancellationToken cancellationToken = default);
}
