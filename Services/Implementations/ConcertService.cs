using Microsoft.EntityFrameworkCore;
using concerts_gate.server.Common.Enums;
using concerts_gate.server.Common.Exceptions;
using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Concerts;
using concerts_gate.server.DTOs.Tickets;
using concerts_gate.server.Entities;
using concerts_gate.server.Repositories.Interfaces;
using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.Services.Implementations;

/// <summary>
/// Implementation of business service for managing Concerts and Ticket Categories.
/// </summary>
public class ConcertService : IConcertService
{
    private readonly IConcertRepository _concertRepository;
    private readonly ITicketCategoryRepository _categoryRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="ConcertService"/>.
    /// </summary>
    public ConcertService(
        IConcertRepository concertRepository,
        ITicketCategoryRepository categoryRepository,
        IAuditLogRepository auditLogRepository)
    {
        _concertRepository = concertRepository;
        _categoryRepository = categoryRepository;
        _auditLogRepository = auditLogRepository;
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<ConcertSummaryDto>> GetPublicConcertsAsync(
        string? search = null,
        string? genre = null,
        bool? onlyFlashSale = null,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _concertRepository.GetPublishedConcerts(search, genre, onlyFlashSale);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ConcertSummaryDto
            {
                Id = c.Id,
                Title = c.Title,
                Artist = c.Artist,
                Venue = c.Venue,
                Genre = c.Genre,
                BannerUrl = c.BannerUrl,
                EventDate = c.EventDate,
                SaleStartDate = c.SaleStartDate,
                SaleEndDate = c.SaleEndDate,
                Status = c.Status,
                IsFlashSale = c.IsFlashSale,
                MinPrice = c.TicketCategories.Any() ? c.TicketCategories.Min(tc => tc.Price) : 0,
                MaxPrice = c.TicketCategories.Any() ? c.TicketCategories.Max(tc => tc.Price) : 0,
                TotalRemainingTickets = c.TicketCategories.Sum(tc => tc.RemainingQuantity)
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<ConcertSummaryDto>(items, totalCount, pageIndex, pageSize);
    }

    /// <inheritdoc />
    public async Task<ConcertDetailDto> GetConcertDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var concert = await _concertRepository.GetWithCategoriesAsync(id, cancellationToken);
        if (concert == null)
        {
            throw new NotFoundException($"Concert not found with ID: {id}");
        }

        return MapToDetailDto(concert);
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<ConcertDetailDto>> GetAllConcertsForAdminAsync(
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _concertRepository.GetAll()
            .Include(c => c.TicketCategories)
            .OrderByDescending(c => c.CreatedAt)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ConcertDetailDto
            {
                Id = c.Id,
                Title = c.Title,
                Artist = c.Artist,
                Venue = c.Venue,
                Genre = c.Genre,
                BannerUrl = c.BannerUrl,
                Description = c.Description,
                EventDate = c.EventDate,
                SaleStartDate = c.SaleStartDate,
                SaleEndDate = c.SaleEndDate,
                Status = c.Status,
                IsFlashSale = c.IsFlashSale,
                MinPrice = c.TicketCategories.Any() ? c.TicketCategories.Min(tc => tc.Price) : 0,
                MaxPrice = c.TicketCategories.Any() ? c.TicketCategories.Max(tc => tc.Price) : 0,
                TotalRemainingTickets = c.TicketCategories.Sum(tc => tc.RemainingQuantity),
                Categories = c.TicketCategories.Select(tc => new TicketCategoryDto
                {
                    Id = tc.Id,
                    ConcertId = tc.ConcertId,
                    Name = tc.Name,
                    Description = tc.Description,
                    Price = tc.Price,
                    TotalQuantity = tc.TotalQuantity,
                    RemainingQuantity = tc.RemainingQuantity,
                    ReservedQuantity = tc.ReservedQuantity,
                    SoldQuantity = tc.SoldQuantity,
                    MaxPerOrder = tc.MaxPerOrder
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<ConcertDetailDto>(items, totalCount, pageIndex, pageSize);
    }

    /// <inheritdoc />
    public async Task<ConcertDetailDto> CreateConcertAsync(CreateConcertDto dto, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (dto.SaleEndDate <= dto.SaleStartDate)
        {
            throw new BadRequestException("Sale end date must be after sale start date.");
        }

        var concert = new Concert
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            Artist = dto.Artist.Trim(),
            Description = dto.Description.Trim(),
            Venue = dto.Venue.Trim(),
            BannerUrl = dto.BannerUrl.Trim(),
            Genre = dto.Genre.Trim(),
            EventDate = dto.EventDate,
            SaleStartDate = dto.SaleStartDate,
            SaleEndDate = dto.SaleEndDate,
            Status = ConcertStatus.Draft,
            IsFlashSale = dto.IsFlashSale,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Categories != null && dto.Categories.Any())
        {
            foreach (var cat in dto.Categories)
            {
                concert.TicketCategories.Add(new TicketCategory
                {
                    Id = Guid.NewGuid(),
                    ConcertId = concert.Id,
                    Name = cat.Name.Trim(),
                    Description = cat.Description.Trim(),
                    Price = cat.Price,
                    TotalQuantity = cat.TotalQuantity,
                    RemainingQuantity = cat.TotalQuantity,
                    ReservedQuantity = 0,
                    SoldQuantity = 0,
                    MaxPerOrder = cat.MaxPerOrder
                });
            }
        }

        await _concertRepository.AddAsync(concert, cancellationToken);
        await _concertRepository.SaveChangesAsync(cancellationToken);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            OperatorId = operatorId,
            Action = "CREATE_CONCERT",
            TargetEntity = nameof(Concert),
            TargetId = concert.Id.ToString(),
            Details = $"Created concert '{concert.Title}' with {concert.TicketCategories.Count} ticket categories.",
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        await _auditLogRepository.SaveChangesAsync(cancellationToken);

        return MapToDetailDto(concert);
    }

    /// <inheritdoc />
    public async Task<ConcertDetailDto> UpdateConcertAsync(Guid id, UpdateConcertDto dto, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var concert = await _concertRepository.GetWithCategoriesAsync(id, cancellationToken);
        if (concert == null)
        {
            throw new NotFoundException($"Concert not found with ID: {id}");
        }

        concert.Title = dto.Title.Trim();
        concert.Artist = dto.Artist.Trim();
        concert.Description = dto.Description.Trim();
        concert.Venue = dto.Venue.Trim();
        concert.BannerUrl = dto.BannerUrl.Trim();
        concert.Genre = dto.Genre.Trim();
        concert.EventDate = dto.EventDate;
        concert.SaleStartDate = dto.SaleStartDate;
        concert.SaleEndDate = dto.SaleEndDate;
        concert.Status = dto.Status;
        concert.IsFlashSale = dto.IsFlashSale;
        concert.UpdatedAt = DateTime.UtcNow;

        _concertRepository.Update(concert);
        await _concertRepository.SaveChangesAsync(cancellationToken);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            OperatorId = operatorId,
            Action = "UPDATE_CONCERT",
            TargetEntity = nameof(Concert),
            TargetId = concert.Id.ToString(),
            Details = $"Updated concert '{concert.Title}', status: {concert.Status}",
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        await _auditLogRepository.SaveChangesAsync(cancellationToken);

        return MapToDetailDto(concert);
    }

    /// <inheritdoc />
    public async Task<bool> ChangeConcertStatusAsync(Guid id, ConcertStatus newStatus, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var concert = await _concertRepository.GetByIdAsync(id, cancellationToken);
        if (concert == null)
        {
            throw new NotFoundException($"Concert not found with ID: {id}");
        }

        var oldStatus = concert.Status;
        concert.Status = newStatus;
        concert.UpdatedAt = DateTime.UtcNow;

        _concertRepository.Update(concert);
        await _concertRepository.SaveChangesAsync(cancellationToken);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            OperatorId = operatorId,
            Action = "CHANGE_CONCERT_STATUS",
            TargetEntity = nameof(Concert),
            TargetId = concert.Id.ToString(),
            Details = $"Changed status of concert '{concert.Title}' from {oldStatus} to {newStatus}.",
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        await _auditLogRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<TicketCategoryDto> AddTicketCategoryAsync(Guid concertId, CreateTicketCategoryDto dto, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var concert = await _concertRepository.GetByIdAsync(concertId, cancellationToken);
        if (concert == null)
        {
            throw new NotFoundException($"Concert not found with ID: {concertId}");
        }

        var category = new TicketCategory
        {
            Id = Guid.NewGuid(),
            ConcertId = concertId,
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Price = dto.Price,
            TotalQuantity = dto.TotalQuantity,
            RemainingQuantity = dto.TotalQuantity,
            ReservedQuantity = 0,
            SoldQuantity = 0,
            MaxPerOrder = dto.MaxPerOrder
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            OperatorId = operatorId,
            Action = "ADD_TICKET_CATEGORY",
            TargetEntity = nameof(TicketCategory),
            TargetId = category.Id.ToString(),
            Details = $"Added category '{category.Name}' (Total: {category.TotalQuantity}, Price: {category.Price}) to concert '{concert.Title}'.",
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        await _auditLogRepository.SaveChangesAsync(cancellationToken);

        return MapCategoryToDto(category);
    }

    /// <inheritdoc />
    public async Task<TicketCategoryDto> UpdateCategoryInventoryAsync(Guid categoryId, UpdateInventoryDto dto, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException($"Ticket category not found with ID: {categoryId}");
        }

        var committedQuantity = category.SoldQuantity + category.ReservedQuantity;
        if (dto.NewTotalQuantity < committedQuantity)
        {
            throw new BadRequestException($"Cannot reduce total tickets to {dto.NewTotalQuantity} because {category.SoldQuantity} tickets are sold and {category.ReservedQuantity} are reserved (Committed: {committedQuantity}).");
        }

        var oldTotal = category.TotalQuantity;
        var diff = dto.NewTotalQuantity - oldTotal;

        category.TotalQuantity = dto.NewTotalQuantity;
        category.RemainingQuantity += diff;

        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            OperatorId = operatorId,
            Action = "UPDATE_INVENTORY",
            TargetEntity = nameof(TicketCategory),
            TargetId = category.Id.ToString(),
            Details = $"Adjusted inventory of category '{category.Name}' from {oldTotal} to {dto.NewTotalQuantity} (Available: {category.RemainingQuantity}).",
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        await _auditLogRepository.SaveChangesAsync(cancellationToken);

        return MapCategoryToDto(category);
    }

    private static ConcertDetailDto MapToDetailDto(Concert concert)
    {
        return new ConcertDetailDto
        {
            Id = concert.Id,
            Title = concert.Title,
            Artist = concert.Artist,
            Description = concert.Description,
            Venue = concert.Venue,
            Genre = concert.Genre,
            BannerUrl = concert.BannerUrl,
            EventDate = concert.EventDate,
            SaleStartDate = concert.SaleStartDate,
            SaleEndDate = concert.SaleEndDate,
            Status = concert.Status,
            IsFlashSale = concert.IsFlashSale,
            MinPrice = concert.TicketCategories.Any() ? concert.TicketCategories.Min(tc => tc.Price) : 0,
            MaxPrice = concert.TicketCategories.Any() ? concert.TicketCategories.Max(tc => tc.Price) : 0,
            TotalRemainingTickets = concert.TicketCategories.Sum(tc => tc.RemainingQuantity),
            Categories = concert.TicketCategories.Select(MapCategoryToDto).ToList()
        };
    }

    private static TicketCategoryDto MapCategoryToDto(TicketCategory tc)
    {
        return new TicketCategoryDto
        {
            Id = tc.Id,
            ConcertId = tc.ConcertId,
            Name = tc.Name,
            Description = tc.Description,
            Price = tc.Price,
            TotalQuantity = tc.TotalQuantity,
            RemainingQuantity = tc.RemainingQuantity,
            ReservedQuantity = tc.ReservedQuantity,
            SoldQuantity = tc.SoldQuantity,
            MaxPerOrder = tc.MaxPerOrder
        };
    }
}
