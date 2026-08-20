using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Vouchers;
using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.Controllers;

/// <summary>
/// Provides customer APIs for validating and applying promotional vouchers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VouchersController : ControllerBase
{
    private readonly IVoucherService _voucherService;

    /// <summary>
    /// Initializes a new instance of <see cref="VouchersController"/>.
    /// </summary>
    public VouchersController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    /// <summary>
    /// Validates a promotional voucher code and computes the discount amount before placing a booking order.
    /// </summary>
    /// <param name="request">Voucher code and order subtotal amount.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Voucher is valid, returns calculated discount amount.</response>
    /// <response code="422">Voucher is invalid, expired, exhausted, or minimum order threshold not met.</response>
    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ValidateVoucherResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ValidateVoucher([FromBody] ValidateVoucherRequestDto request, CancellationToken cancellationToken)
    {
        Guid? userId = null;
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(sub) && Guid.TryParse(sub, out var parsedGuid))
        {
            userId = parsedGuid;
        }

        var result = await _voucherService.ValidateAndCalculateDiscountAsync(request.Code, userId ?? Guid.Empty, request.OrderAmount, cancellationToken);
        return Ok(ApiResponse<ValidateVoucherResponseDto>.Ok(result, "Voucher code is valid!"));
    }
}
