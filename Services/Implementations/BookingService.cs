using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using concerts_gate.server.Common.Constants;
using concerts_gate.server.Common.Enums;
using concerts_gate.server.Common.Exceptions;
using concerts_gate.server.Common.Models;
using concerts_gate.server.Data;
using concerts_gate.server.DTOs.Bookings;
using concerts_gate.server.Entities;
using concerts_gate.server.Repositories.Interfaces;
using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.Services.Implementations;

/// <summary>
/// Implementation of the ticket booking engine, Flash Sale concurrency control, and ticket payment processing.
/// </summary>
public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IBookingRepository _bookingRepository;
    private readonly IConcertRepository _concertRepository;
    private readonly ITicketCategoryRepository _categoryRepository;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IVoucherService _voucherService;
    private readonly IIdempotencyRepository _idempotencyRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="BookingService"/>.
    /// </summary>
    public BookingService(
        ApplicationDbContext dbContext,
        IBookingRepository bookingRepository,
        IConcertRepository concertRepository,
        ITicketCategoryRepository categoryRepository,
        IVoucherRepository voucherRepository,
        IVoucherService voucherService,
        IIdempotencyRepository idempotencyRepository,
        IAuditLogRepository auditLogRepository)
    {
        _dbContext = dbContext;
        _bookingRepository = bookingRepository;
        _concertRepository = concertRepository;
        _categoryRepository = categoryRepository;
        _voucherRepository = voucherRepository;
        _voucherService = voucherService;
        _idempotencyRepository = idempotencyRepository;
        _auditLogRepository = auditLogRepository;
    }

    /// <inheritdoc />
    public async Task<BookingResponseDto> CreateBookingAsync(
        CreateBookingRequestDto request,
        Guid userId,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Check Idempotency Key (prevents duplicate orders upon client retries)
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existingRecord = await _idempotencyRepository.GetByKeyAsync(idempotencyKey.Trim(), userId, cancellationToken);
            if (existingRecord != null)
            {
                // If previously processed, deserialize and return cached response immediately
                if (!string.IsNullOrEmpty(existingRecord.ResponseBody))
                {
                    try
                    {
                        var cachedResult = JsonSerializer.Deserialize<BookingResponseDto>(existingRecord.ResponseBody);
                        if (cachedResult != null) return cachedResult;
                    }
                    catch
                    {
                        // Fallback if cache parsing fails
                    }
                }
            }
        }

        // 2. Validate Concert status and sale window
        var concert = await _concertRepository.GetWithCategoriesAsync(request.ConcertId, cancellationToken);
        if (concert == null)
        {
            throw new NotFoundException($"Concert not found with ID: {request.ConcertId}");
        }

        if (concert.Status != ConcertStatus.Published)
        {
            throw new BadRequestException("This concert is not currently published for ticket sales.");
        }

        var now = DateTime.UtcNow;
        if (now < concert.SaleStartDate)
        {
            throw new BadRequestException($"Ticket sales for this concert will start at: {concert.SaleStartDate:yyyy-MM-dd HH:mm} UTC.");
        }

        if (now > concert.SaleEndDate)
        {
            throw new BadRequestException("Ticket sales for this concert have concluded.");
        }

        // 3. Validate total ticket count per order (bot/scalping prevention)
        var totalRequestedTickets = request.Items.Sum(i => i.Quantity);
        if (totalRequestedTickets <= 0)
        {
            throw new BadRequestException("Requested ticket quantity must be greater than 0.");
        }
        if (totalRequestedTickets > AppConstants.BookingConfig.MaxTicketsPerOrder)
        {
            throw new BadRequestException($"You can only reserve up to {AppConstants.BookingConfig.MaxTicketsPerOrder} tickets per order.");
        }

        // 4. Initialize Transaction & Process Inventory with Optimistic Concurrency
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                decimal originalAmount = 0;
                var bookingItems = new List<BookingItem>();
                var categoryUpdates = new List<TicketCategory>();

                // Check each category and decrement available inventory (RemainingQuantity)
                foreach (var itemReq in request.Items)
                {
                    var category = await _dbContext.TicketCategories
                        .FirstOrDefaultAsync(tc => tc.Id == itemReq.TicketCategoryId && tc.ConcertId == concert.Id, cancellationToken);

                    if (category == null)
                    {
                        throw new BadRequestException($"Ticket category '{itemReq.TicketCategoryId}' does not belong to this concert.");
                    }

                    if (itemReq.Quantity > category.MaxPerOrder)
                    {
                        throw new BadRequestException($"Ticket category '{category.Name}' only permits up to {category.MaxPerOrder} tickets per order.");
                    }

                    // CHECK AVAILABLE INVENTORY (Anti-Overselling)
                    if (category.RemainingQuantity < itemReq.Quantity)
                    {
                        throw new ConcurrencyException($"Ticket category '{category.Name}' has insufficient available seats (Remaining: {category.RemainingQuantity}, Requested: {itemReq.Quantity}).");
                    }

                    // Temporary reservation: Decrement RemainingQuantity, Increment ReservedQuantity
                    category.RemainingQuantity -= itemReq.Quantity;
                    category.ReservedQuantity += itemReq.Quantity;
                    categoryUpdates.Add(category);

                    var subtotal = category.Price * itemReq.Quantity;
                    originalAmount += subtotal;

                    bookingItems.Add(new BookingItem
                    {
                        Id = Guid.NewGuid(),
                        TicketCategoryId = category.Id,
                        Quantity = itemReq.Quantity,
                        UnitPrice = category.Price,
                        TicketCategory = category
                    });
                }

                // 5. Process Promotional Voucher if provided (Voucher Abuse Prevention)
                decimal discountAmount = 0;
                Voucher? appliedVoucher = null;

                if (!string.IsNullOrWhiteSpace(request.VoucherCode))
                {
                    var voucherValidation = await _voucherService.ValidateAndCalculateDiscountAsync(
                        request.VoucherCode.Trim(),
                        userId,
                        originalAmount,
                        cancellationToken);

                    discountAmount = voucherValidation.DiscountAmount;
                    appliedVoucher = await _voucherRepository.GetByCodeAsync(request.VoucherCode.Trim(), cancellationToken);

                    if (appliedVoucher != null)
                    {
                        // Increment actual usage count of the voucher
                        appliedVoucher.CurrentUsageCount += 1;
                        _voucherRepository.Update(appliedVoucher);
                    }
                }

                var finalAmount = Math.Max(0, originalAmount - discountAmount);

                // 6. Create new Booking order
                var bookingCode = $"CG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
                var expiresAt = DateTime.UtcNow.AddMinutes(AppConstants.BookingConfig.ReservationTtlMinutes);

                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    BookingCode = bookingCode,
                    UserId = userId,
                    ConcertId = concert.Id,
                    OriginalAmount = originalAmount,
                    DiscountAmount = discountAmount,
                    FinalAmount = finalAmount,
                    Status = BookingStatus.PendingPayment,
                    IdempotencyKey = idempotencyKey?.Trim(),
                    AppliedVoucherCode = appliedVoucher?.Code,
                    ReservationExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    BookingItems = bookingItems
                };

                foreach (var item in bookingItems)
                {
                    item.BookingId = booking.Id;
                }

                await _bookingRepository.AddAsync(booking, cancellationToken);

                // Record VoucherUsage
                if (appliedVoucher != null && discountAmount > 0)
                {
                    await _voucherRepository.AddUsageAsync(new VoucherUsage
                    {
                        Id = Guid.NewGuid(),
                        VoucherId = appliedVoucher.Id,
                        UserId = userId,
                        BookingId = booking.Id,
                        DiscountApplied = discountAmount,
                        UsedAt = DateTime.UtcNow
                    }, cancellationToken);
                }

                // 7. Save Idempotency Record
                var responseDto = MapToResponseDto(booking, concert);

                if (!string.IsNullOrWhiteSpace(idempotencyKey))
                {
                    var idempotencyRecord = new IdempotencyRecord
                    {
                        Id = Guid.NewGuid(),
                        Key = idempotencyKey.Trim(),
                        UserId = userId,
                        RequestPath = "/api/bookings",
                        StatusCode = 200,
                        ResponseBody = JsonSerializer.Serialize(responseDto),
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddHours(AppConstants.BookingConfig.IdempotencyTtlHours)
                    };
                    await _idempotencyRepository.AddAsync(idempotencyRecord, cancellationToken);
                }

                // Persist changes (with EF Core automatic Concurrency Token validation)
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return responseDto;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                // Row version conflict due to multiple users racing for the last tickets
                throw new ConcurrencyException("Tickets were booked by another user just now. Please reload and try again!");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <inheritdoc />
    public async Task<BookingResponseDto> ProcessPaymentAsync(
        Guid bookingId,
        Guid userId,
        PaymentSimulationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetDetailedByIdAsync(bookingId, cancellationToken);
        if (booking == null)
        {
            throw new NotFoundException($"Booking not found with ID: {bookingId}");
        }

        if (booking.UserId != userId)
        {
            throw new BadRequestException("You do not have permission to pay for another user's booking.");
        }

        if (booking.Status != BookingStatus.PendingPayment)
        {
            throw new BadRequestException($"Cannot process payment for a booking with status '{booking.Status}'.");
        }

        if (DateTime.UtcNow > booking.ReservationExpiresAt)
        {
            // Auto-transition to Expired and release tickets
            await CancelBookingAsync(booking.Id, null, "Payment hold time expired (TTL expired).", cancellationToken);
            throw new BadRequestException("Booking reservation has expired and tickets were released. Please create a new booking.");
        }

        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Update booking status to Confirmed
                booking.Status = BookingStatus.Confirmed;
                booking.PaidAt = DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.Notes = $"Payment successful via {request.PaymentMethod}. Ref: {request.TransactionReference ?? Guid.NewGuid().ToString("N")[..12]}";

                // Shift ticket quantities from Reserved to Sold
                var ticketsToIssue = new List<Ticket>();

                foreach (var item in booking.BookingItems)
                {
                    var category = await _dbContext.TicketCategories.FindAsync(new object[] { item.TicketCategoryId }, cancellationToken);
                    if (category != null)
                    {
                        category.ReservedQuantity = Math.Max(0, category.ReservedQuantity - item.Quantity);
                        category.SoldQuantity += item.Quantity;
                        _categoryRepository.Update(category);
                    }

                    // Issue individual electronic tickets with QR codes
                    for (int i = 1; i <= item.Quantity; i++)
                    {
                        var ticketCode = $"TKT-{booking.ConcertId.ToString()[..4].ToUpper()}-{category?.Name[..3].ToUpper() ?? "VIP"}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                        var qrPayload = $"CG-PASS:{ticketCode}:BOOKING={booking.BookingCode}:CAT={item.TicketCategoryId}";

                        ticketsToIssue.Add(new Ticket
                        {
                            Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            TicketCategoryId = item.TicketCategoryId,
                            TicketCode = ticketCode,
                            QrCodePayload = qrPayload,
                            Status = TicketStatus.Valid,
                            IssuedAt = DateTime.UtcNow,
                            TicketCategory = category!
                        });
                    }
                }

                await _bookingRepository.AddTicketsAsync(ticketsToIssue, cancellationToken);
                _bookingRepository.Update(booking);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                booking.Tickets = ticketsToIssue;
                return MapToResponseDto(booking, booking.Concert);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <inheritdoc />
    public async Task<bool> CancelBookingAsync(
        Guid bookingId,
        Guid? userId,
        string reason = "Cancelled by customer.",
        CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetDetailedByIdAsync(bookingId, cancellationToken);
        if (booking == null)
        {
            throw new NotFoundException($"Booking not found with ID: {bookingId}");
        }

        if (userId.HasValue && booking.UserId != userId.Value)
        {
            throw new BadRequestException("You do not have permission to modify this booking.");
        }

        if (booking.Status != BookingStatus.PendingPayment)
        {
            throw new BadRequestException($"Only bookings in 'PendingPayment' status can be cancelled. Current status: {booking.Status}");
        }

        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.Notes = reason;

                // Return reserved tickets to available inventory: Decrement ReservedQuantity, Increment RemainingQuantity
                foreach (var item in booking.BookingItems)
                {
                    var category = await _dbContext.TicketCategories.FindAsync(new object[] { item.TicketCategoryId }, cancellationToken);
                    if (category != null)
                    {
                        category.ReservedQuantity = Math.Max(0, category.ReservedQuantity - item.Quantity);
                        category.RemainingQuantity += item.Quantity;
                        _categoryRepository.Update(category);
                    }
                }

                // If a voucher was applied, restore the usage count
                if (!string.IsNullOrEmpty(booking.AppliedVoucherCode))
                {
                    var voucher = await _voucherRepository.GetByCodeAsync(booking.AppliedVoucherCode, cancellationToken);
                    if (voucher != null && voucher.CurrentUsageCount > 0)
                    {
                        voucher.CurrentUsageCount -= 1;
                        _voucherRepository.Update(voucher);
                    }
                }

                _bookingRepository.Update(booking);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <inheritdoc />
    public async Task<BookingResponseDto> GetBookingByIdAsync(Guid bookingId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetDetailedByIdAsync(bookingId, cancellationToken);
        if (booking == null)
        {
            throw new NotFoundException($"Booking not found with ID: {bookingId}");
        }

        if (userId.HasValue && booking.UserId != userId.Value)
        {
            throw new BadRequestException("You do not have permission to view this booking.");
        }

        return MapToResponseDto(booking, booking.Concert);
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<BookingResponseDto>> GetMyBookingsAsync(
        Guid userId,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _bookingRepository.GetAll()
            .Include(b => b.Concert)
            .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.TicketCategory)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.TicketCategory)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(b => MapToResponseDto(b, b.Concert)).ToList();

        return new PaginatedResult<BookingResponseDto>(dtos, totalCount, pageIndex, pageSize);
    }

    /// <inheritdoc />
    public async Task<int> ReleaseExpiredBookingsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiredBookings = await _bookingRepository.GetExpiredPendingBookingsAsync(now, cancellationToken);

        if (!expiredBookings.Any()) return 0;

        int count = 0;
        foreach (var booking in expiredBookings)
        {
            try
            {
                booking.Status = BookingStatus.Expired;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.Notes = "Automatically expired by Background Worker due to reservation TTL.";

                // Restore ticket inventory
                foreach (var item in booking.BookingItems)
                {
                    if (item.TicketCategory != null)
                    {
                        item.TicketCategory.ReservedQuantity = Math.Max(0, item.TicketCategory.ReservedQuantity - item.Quantity);
                        item.TicketCategory.RemainingQuantity += item.Quantity;
                        _categoryRepository.Update(item.TicketCategory);
                    }
                }

                // Restore voucher usage count if applicable
                if (!string.IsNullOrEmpty(booking.AppliedVoucherCode))
                {
                    var voucher = await _voucherRepository.GetByCodeAsync(booking.AppliedVoucherCode, cancellationToken);
                    if (voucher != null && voucher.CurrentUsageCount > 0)
                    {
                        voucher.CurrentUsageCount -= 1;
                        _voucherRepository.Update(voucher);
                    }
                }

                _bookingRepository.Update(booking);
                count++;
            }
            catch
            {
                // Ignore individual errors so the worker loop is not interrupted
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return count;
    }

    private static BookingResponseDto MapToResponseDto(Booking booking, Concert concert)
    {
        return new BookingResponseDto
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            ConcertId = booking.ConcertId,
            ConcertTitle = concert?.Title ?? booking.Concert?.Title ?? "N/A",
            Artist = concert?.Artist ?? booking.Concert?.Artist ?? "N/A",
            Venue = concert?.Venue ?? booking.Concert?.Venue ?? "N/A",
            EventDate = concert?.EventDate ?? booking.Concert?.EventDate ?? DateTime.MinValue,
            OriginalAmount = booking.OriginalAmount,
            DiscountAmount = booking.DiscountAmount,
            FinalAmount = booking.FinalAmount,
            Status = booking.Status,
            AppliedVoucherCode = booking.AppliedVoucherCode,
            ReservationExpiresAt = booking.ReservationExpiresAt,
            PaidAt = booking.PaidAt,
            CreatedAt = booking.CreatedAt,
            Notes = booking.Notes,
            Items = booking.BookingItems.Select(bi => new BookingItemDto
            {
                Id = bi.Id,
                TicketCategoryId = bi.TicketCategoryId,
                CategoryName = bi.TicketCategory?.Name ?? "N/A",
                Quantity = bi.Quantity,
                UnitPrice = bi.UnitPrice
            }).ToList(),
            Tickets = booking.Tickets.Select(t => new TicketDto
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
