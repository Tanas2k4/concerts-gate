using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using concerts_gate.server.Common.Constants;
using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Vouchers;
using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.Controllers.Operations;

/// <summary>
/// Provides administrative APIs for managing and creating promotional voucher campaigns.
/// </summary>
[ApiController]
[Route("api/admin/vouchers")]
[Authorize(Roles = AppConstants.Roles.InternalOperations)]
[Produces("application/json")]
public class AdminVouchersController : ControllerBase
{
    private readonly IVoucherService _voucherService;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminVouchersController"/>.
    /// </summary>
    public AdminVouchersController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    /// <summary>
    /// Retrieves a paginated list of all vouchers with usage statistics and active status.
    /// </summary>
    /// <param name="pageIndex">Page index (default 1).</param>
    /// <param name="pageSize">Items per page (default 20).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns paginated vouchers list.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<VoucherDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllVouchers(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _voucherService.GetAllVouchersAsync(pageIndex, pageSize, cancellationToken);
        return Ok(ApiResponse<PaginatedResult<VoucherDto>>.Ok(result));
    }

    /// <summary>
    /// Creates a new promotional voucher campaign with global and per-account usage limits.
    /// </summary>
    /// <param name="dto">Voucher configuration parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Voucher created successfully.</response>
    /// <response code="400">Voucher code exists or invalid date range.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VoucherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherDto dto, CancellationToken cancellationToken)
    {
        var operatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _voucherService.CreateVoucherAsync(dto, operatorId, cancellationToken);
        return Ok(ApiResponse<VoucherDto>.Ok(result, "Voucher campaign created successfully!"));
    }

    /// <summary>
    /// Toggles the active status of a voucher (pause or reactivate campaign).
    /// </summary>
    /// <param name="id">Voucher unique identifier (GUID).</param>
    /// <param name="isActive">Active status (true = active, false = paused).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Voucher status toggled successfully.</response>
    [HttpPatch("{id:guid}/toggle-status")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromQuery] bool isActive, CancellationToken cancellationToken)
    {
        var operatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _voucherService.ToggleVoucherStatusAsync(id, isActive, operatorId, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, $"Voucher successfully {(isActive ? "activated" : "deactivated")}!"));
    }
}
