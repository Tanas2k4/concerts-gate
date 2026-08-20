using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Concerts;
using concerts_gate.server.DTOs.Tickets;

namespace concerts_gate.server.Services.Interfaces;

/// <summary>
/// Provides business methods for managing Concerts and Ticket Categories.
/// </summary>
public interface IConcertService
{
    /// <summary>
    /// Retrieves a paginated list of published concerts with filtering and search capabilities for customers.
    /// </summary>
    /// <param name="search">Search keyword across title, artist, or venue.</param>
    /// <param name="genre">Music genre.</param>
    /// <param name="onlyFlashSale">Filter exclusively for Flash Sale concerts.</param>
    /// <param name="pageIndex">Current page index (1-indexed).</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated result <see cref="PaginatedResult{ConcertSummaryDto}"/>.</returns>
    Task<PaginatedResult<ConcertSummaryDto>> GetPublicConcertsAsync(
        string? search = null,
        string? genre = null,
        bool? onlyFlashSale = null,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves full concert details including available ticket categories.
    /// </summary>
    /// <param name="id">Concert ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="ConcertDetailDto"/> or throws <see cref="Common.Exceptions.NotFoundException"/>.</returns>
    Task<ConcertDetailDto> GetConcertDetailAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all concerts (including drafts and archived) for the operations dashboard.
    /// </summary>
    /// <param name="pageIndex">Current page index.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated result of concert details.</returns>
    Task<PaginatedResult<ConcertDetailDto>> GetAllConcertsForAdminAsync(
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new concert event and optional initial ticket categories.
    /// </summary>
    /// <param name="dto">Concert creation payload.</param>
    /// <param name="operatorId">Operator ID performing the creation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created concert details.</returns>
    Task<ConcertDetailDto> CreateConcertAsync(CreateConcertDto dto, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates details of an existing concert.
    /// </summary>
    /// <param name="id">Concert ID to update.</param>
    /// <param name="dto">Update payload.</param>
    /// <param name="operatorId">Operator ID performing the update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated concert details.</returns>
    Task<ConcertDetailDto> UpdateConcertAsync(Guid id, UpdateConcertDto dto, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the publication status of a concert (Publish / Unpublish / Archive).
    /// </summary>
    /// <param name="id">Concert ID.</param>
    /// <param name="newStatus">New publication status.</param>
    /// <param name="operatorId">Operator ID performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successfully updated.</returns>
    Task<bool> ChangeConcertStatusAsync(Guid id, Common.Enums.ConcertStatus newStatus, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new ticket category tier to an existing concert.
    /// </summary>
    /// <param name="concertId">Concert ID.</param>
    /// <param name="dto">Ticket category creation payload.</param>
    /// <param name="operatorId">Operator ID performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created ticket category details.</returns>
    Task<TicketCategoryDto> AddTicketCategoryAsync(Guid concertId, CreateTicketCategoryDto dto, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adjusts the total allocated ticket inventory for a category.
    /// </summary>
    /// <param name="categoryId">Ticket category ID.</param>
    /// <param name="dto">New total inventory quantity.</param>
    /// <param name="operatorId">Operator ID performing the update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated ticket category details.</returns>
    Task<TicketCategoryDto> UpdateCategoryInventoryAsync(Guid categoryId, UpdateInventoryDto dto, Guid operatorId, CancellationToken cancellationToken = default);
}
