using Microsoft.AspNetCore.Mvc;
using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Concerts;
using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.Controllers;

/// <summary>
/// Provides public APIs for browsing concert catalogs and viewing concert event details.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ConcertsController : ControllerBase
{
    private readonly IConcertService _concertService;

    /// <summary>
    /// Initializes a new instance of <see cref="ConcertsController"/>.
    /// </summary>
    /// <param name="concertService">Concert management service.</param>
    public ConcertsController(IConcertService concertService)
    {
        _concertService = concertService;
    }

    /// <summary>
    /// Retrieves a paginated list of published concerts available for sale (supports searching, genre filtering, and Flash Sale filter).
    /// </summary>
    /// <param name="search">Search keyword across concert title, artist, or venue.</param>
    /// <param name="genre">Music genre (Pop, Rock, EDM, etc.).</param>
    /// <param name="onlyFlashSale">Filter exclusively for Flash Sale events.</param>
    /// <param name="pageIndex">Page index (default 1).</param>
    /// <param name="pageSize">Number of concerts per page (default 10).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns paginated list of concerts.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ConcertSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConcerts(
        [FromQuery] string? search,
        [FromQuery] string? genre,
        [FromQuery] bool? onlyFlashSale,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _concertService.GetPublicConcertsAsync(search, genre, onlyFlashSale, pageIndex, pageSize, cancellationToken);
        return Ok(ApiResponse<PaginatedResult<ConcertSummaryDto>>.Ok(result));
    }

    /// <summary>
    /// Retrieves full details of a concert event including ticket categories (VIP, Standard, etc.) and remaining seat counts.
    /// </summary>
    /// <param name="id">Unique identifier of the concert (GUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns concert details.</response>
    /// <response code="404">Concert not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ConcertDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConcertById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _concertService.GetConcertDetailAsync(id, cancellationToken);
        return Ok(ApiResponse<ConcertDetailDto>.Ok(result));
    }
}
