using Microsoft.EntityFrameworkCore;
using concerts_gate.server.Common.Enums;
using concerts_gate.server.Common.Exceptions;
using concerts_gate.server.Common.Models;
using concerts_gate.server.Data;
using concerts_gate.server.DTOs.Bookings;
using concerts_gate.server.DTOs.Operations;
using concerts_gate.server.Entities;
using concerts_gate.server.Repositories.Interfaces;
using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.Services.Implementations;

/// <summary>
/// Implementation of internal operations management service (Operation Dashboard &amp; Admin workflows).
/// </summary>
public class OperationService : IOperationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IBookingRepository _bookingRepository;
    private readonly IConcertRepository _concertRepository;
    private readonly ITicketCategoryRepository _categoryRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="OperationService"/>.
    /// </summary>
    public OperationService(
        ApplicationDbContext dbContext,
        IBookingRepository bookingRepository,
        IConcertRepository concertRepository,
        ITicketCategoryRepository categoryRepository,
        IAuditLogRepository auditLogRepository)
    {
        _dbContext = dbContext;
        _bookingRepository = bookingRepository;
        _concertRepository = concertRepository;
        _categoryRepository = categoryRepository;
        _auditLogRepository = auditLogRepository;
    }

    /// <inheritdoc />
    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var bookings = await _dbContext.Bookings.AsNoTracking().ToListAsync(cancellationToken);
        var categories = await _dbContext.TicketCategories.AsNoTracking().ToListAsync(cancellationToken);

        var totalRevenue = bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .Sum(b => b.FinalAmount);

        var totalSold = categories.Sum(c => c.SoldQuantity);
        var totalReserved = categories.Sum(c => c.ReservedQuantity);

        var topConcerts = await _dbContext.Concerts
            .Include(c => c.TicketCategories)
            .Select(c => new TopConcertStatsDto
            {
                ConcertId = c.Id,
                Title = c.Title,
                TotalCapacity = c.TicketCategories.Sum(tc => tc.TotalQuantity),
                SoldCount = c.TicketCategories.Sum(tc => tc.SoldQuantity),
                Revenue = c.Bookings.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.FinalAmount)
            })
            .OrderByDescending(c => c.Revenue)
            .Take(5)
            .ToListAsync(cancellationToken);

        return new DashboardStatsDto
        {
            TotalRevenue = totalRevenue,
            TotalTicketsSold = totalSold,
            TotalTicketsReserved = totalReserved,
            TotalBookingsCount = bookings.Count,
            PendingBookingsCount = bookings.Count(b => b.Status == BookingStatus.PendingPayment),
            ConfirmedBookingsCount = bookings.Count(b => b.Status == BookingStatus.Confirmed),
            ExpiredBookingsCount = bookings.Count(b => b.Status == BookingStatus.Expired),
            SuspiciousBookingsCount = bookings.Count(b => b.Status == BookingStatus.Suspicious),
            CancelledBookingsCount = bookings.Count(b => b.Status == BookingStatus.Cancelled),
            TopConcerts = topConcerts
        };
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<BookingResponseDto>> GetBookingsForMonitoringAsync(
        BookingStatus? status = null,
        Guid? concertId = null,
        string? search = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _bookingRepository.GetAll()
            .Include(b => b.User)
            .Include(b => b.Concert)
            .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.TicketCategory)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.TicketCategory)
            .AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        if (concertId.HasValue)
        {
            query = query.Where(b => b.ConcertId == concertId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(b => b.BookingCode.ToLower().Contains(s) ||
                                     b.User.Email!.ToLower().Contains(s) ||
                                     b.User.FullName.ToLower().Contains(s) ||
                                     b.Concert.Title.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(b => new BookingResponseDto
        {
            Id = b.Id,
            BookingCode = b.BookingCode,
            ConcertId = b.ConcertId,
            ConcertTitle = b.Concert?.Title ?? "N/A",
            Artist = b.Concert?.Artist ?? "N/A",
            Venue = b.Concert?.Venue ?? "N/A",
            EventDate = b.Concert?.EventDate ?? DateTime.MinValue,
            OriginalAmount = b.OriginalAmount,
            DiscountAmount = b.DiscountAmount,
            FinalAmount = b.FinalAmount,
            Status = b.Status,
            AppliedVoucherCode = b.AppliedVoucherCode,
            ReservationExpiresAt = b.ReservationExpiresAt,
            PaidAt = b.PaidAt,
            CreatedAt = b.CreatedAt,
            Notes = b.Notes,
            Items = b.BookingItems.Select(bi => new BookingItemDto
            {
                Id = bi.Id,
                TicketCategoryId = bi.TicketCategoryId,
                CategoryName = bi.TicketCategory?.Name ?? "N/A",
                Quantity = bi.Quantity,
                UnitPrice = bi.UnitPrice
            }).ToList(),
            Tickets = b.Tickets.Select(t => new TicketDto
            {
                Id = t.Id,
                TicketCode = t.TicketCode,
                QrCodePayload = t.QrCodePayload,
                CategoryName = t.TicketCategory?.Name ?? "N/A",
                Status = t.Status,
                IssuedAt = t.IssuedAt
            }).ToList()
        }).ToList();

        return new PaginatedResult<BookingResponseDto>(dtos, totalCount, pageIndex, pageSize);
    }

    /// <inheritdoc />
    public async Task<BookingResponseDto> UpdateBookingStatusManuallyAsync(
        Guid bookingId,
        UpdateBookingStatusDto dto,
        Guid operatorId,
        CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetDetailedByIdAsync(bookingId, cancellationToken);
        if (booking == null)
        {
            throw new NotFoundException($"Booking not found with ID: {bookingId}");
        }

        var oldStatus = booking.Status;
        var newStatus = dto.NewStatus;

        if (oldStatus == newStatus)
        {
            throw new BadRequestException($"Booking is already in '{newStatus}' status.");
        }

        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Handle ticket restoration or confirmation depending on transition
                if (newStatus == BookingStatus.Cancelled || newStatus == BookingStatus.Refunded)
                {
                    if (oldStatus == BookingStatus.PendingPayment)
                    {
                        // On hold -> release back to available quantity
                        foreach (var item in booking.BookingItems)
                        {
                            var cat = await _dbContext.TicketCategories.FindAsync(new object[] { item.TicketCategoryId }, cancellationToken);
                            if (cat != null)
                            {
                                cat.ReservedQuantity = Math.Max(0, cat.ReservedQuantity - item.Quantity);
                                cat.RemainingQuantity += item.Quantity;
                                _categoryRepository.Update(cat);
                            }
                        }
                    }
                    else if (oldStatus == BookingStatus.Confirmed)
                    {
                        // Sold -> revoke tickets and return to available pool
                        foreach (var item in booking.BookingItems)
                        {
                            var cat = await _dbContext.TicketCategories.FindAsync(new object[] { item.TicketCategoryId }, cancellationToken);
                            if (cat != null)
                            {
                                cat.SoldQuantity = Math.Max(0, cat.SoldQuantity - item.Quantity);
                                cat.RemainingQuantity += item.Quantity;
                                _categoryRepository.Update(cat);
                            }
                        }

                        // Revoke individual electronic tickets
                        foreach (var ticket in booking.Tickets)
                        {
                            ticket.Status = TicketStatus.Revoked;
                        }
                    }
                }
                else if (newStatus == BookingStatus.Confirmed && oldStatus == BookingStatus.PendingPayment)
                {
                    // Transition from Pending to Confirmed: Convert Reserved to Sold & issue tickets
                    foreach (var item in booking.BookingItems)
                    {
                        var cat = await _dbContext.TicketCategories.FindAsync(new object[] { item.TicketCategoryId }, cancellationToken);
                        if (cat != null)
                        {
                            cat.ReservedQuantity = Math.Max(0, cat.ReservedQuantity - item.Quantity);
                            cat.SoldQuantity += item.Quantity;
                            _categoryRepository.Update(cat);
                        }

                        if (!booking.Tickets.Any(t => t.TicketCategoryId == item.TicketCategoryId))
                        {
                            for (int i = 1; i <= item.Quantity; i++)
                            {
                                var ticketCode = $"TKT-MANUAL-{booking.ConcertId.ToString()[..4].ToUpper()}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                                var qrPayload = $"CG-PASS:{ticketCode}:BOOKING={booking.BookingCode}";

                                var t = new Ticket
                                {
                                    Id = Guid.NewGuid(),
                                    BookingId = booking.Id,
                                    TicketCategoryId = item.TicketCategoryId,
                                    TicketCode = ticketCode,
                                    QrCodePayload = qrPayload,
                                    Status = TicketStatus.Valid,
                                    IssuedAt = DateTime.UtcNow
                                };
                                await _dbContext.Tickets.AddAsync(t, cancellationToken);
                            }
                        }
                    }
                    booking.PaidAt = DateTime.UtcNow;
                }

                booking.Status = newStatus;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.Notes = $"[Operator Manual Action]: {dto.Reason}";

                _bookingRepository.Update(booking);

                await _auditLogRepository.AddAsync(new AuditLog
                {
                    OperatorId = operatorId,
                    Action = "MANUAL_UPDATE_BOOKING_STATUS",
                    TargetEntity = nameof(Booking),
                    TargetId = booking.Id.ToString(),
                    Details = $"Changed booking {booking.BookingCode} status from {oldStatus} to {newStatus}. Reason: {dto.Reason}",
                    Timestamp = DateTime.UtcNow
                }, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return await GetBookingDtoAsync(booking.Id, cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <inheritdoc />
    public async Task<BookingResponseDto> FlagSuspiciousBookingAsync(
        Guid bookingId,
        FlagSuspiciousBookingDto dto,
        Guid operatorId,
        CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetDetailedByIdAsync(bookingId, cancellationToken);
        if (booking == null)
        {
            throw new NotFoundException($"Booking not found with ID: {bookingId}");
        }

        var oldStatus = booking.Status;
        if (dto.IsSuspicious)
        {
            booking.Status = BookingStatus.Suspicious;
            booking.Notes = $"[FLAGGED SUSPICIOUS]: {dto.Reason}";
        }
        else
        {
            // Clear flag: return to Pending if unpaid, or Confirmed if paid
            booking.Status = booking.PaidAt.HasValue ? BookingStatus.Confirmed : BookingStatus.PendingPayment;
            booking.Notes = $"[UNFLAGGED SAFE]: {dto.Reason}";
        }

        booking.UpdatedAt = DateTime.UtcNow;
        _bookingRepository.Update(booking);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            OperatorId = operatorId,
            Action = dto.IsSuspicious ? "FLAG_SUSPICIOUS" : "UNFLAG_SUSPICIOUS",
            TargetEntity = nameof(Booking),
            TargetId = booking.Id.ToString(),
            Details = $"Updated suspicious state of booking {booking.BookingCode} from {oldStatus} to {booking.Status}. Reason: {dto.Reason}",
            Timestamp = DateTime.UtcNow
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetBookingDtoAsync(booking.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<InventoryValidationReportDto> ValidateConcertInventoryAsync(
        Guid concertId,
        CancellationToken cancellationToken = default)
    {
        var concert = await _concertRepository.GetWithCategoriesAsync(concertId, cancellationToken);
        if (concert == null)
        {
            throw new NotFoundException($"Concert not found with ID: {concertId}");
        }

        var checkList = concert.TicketCategories.Select(tc => new CategoryInventoryCheckDto
        {
            CategoryId = tc.Id,
            CategoryName = tc.Name,
            TotalQuantity = tc.TotalQuantity,
            RemainingQuantity = tc.RemainingQuantity,
            ReservedQuantity = tc.ReservedQuantity,
            SoldQuantity = tc.SoldQuantity
        }).ToList();

        return new InventoryValidationReportDto
        {
            ConcertId = concert.Id,
            ConcertTitle = concert.Title,
            Categories = checkList
        };
    }

    private async Task<BookingResponseDto> GetBookingDtoAsync(Guid id, CancellationToken cancellationToken)
    {
        var b = await _bookingRepository.GetDetailedByIdAsync(id, cancellationToken);
        return new BookingResponseDto
        {
            Id = b!.Id,
            BookingCode = b.BookingCode,
            ConcertId = b.ConcertId,
            ConcertTitle = b.Concert?.Title ?? "N/A",
            Artist = b.Concert?.Artist ?? "N/A",
            Venue = b.Concert?.Venue ?? "N/A",
            EventDate = b.Concert?.EventDate ?? DateTime.MinValue,
            OriginalAmount = b.OriginalAmount,
            DiscountAmount = b.DiscountAmount,
            FinalAmount = b.FinalAmount,
            Status = b.Status,
            AppliedVoucherCode = b.AppliedVoucherCode,
            ReservationExpiresAt = b.ReservationExpiresAt,
            PaidAt = b.PaidAt,
            CreatedAt = b.CreatedAt,
            Notes = b.Notes,
            Items = b.BookingItems.Select(bi => new BookingItemDto
            {
                Id = bi.Id,
                TicketCategoryId = bi.TicketCategoryId,
                CategoryName = bi.TicketCategory?.Name ?? "N/A",
                Quantity = bi.Quantity,
                UnitPrice = bi.UnitPrice
            }).ToList(),
            Tickets = b.Tickets.Select(t => new TicketDto
            {
                Id = t.Id,
                TicketCode = t.TicketCode,
                QrCodePayload = t.QrCodePayload,
                CategoryName = t.TicketCategory?.Name ?? "N/A",
                Status = t.Status,
                IssuedAt = t.IssuedAt
            }).ToList()
        };
    }
}
