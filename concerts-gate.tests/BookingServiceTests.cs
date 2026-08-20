using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using concerts_gate.server.Common.Enums;
using concerts_gate.server.Common.Exceptions;
using concerts_gate.server.Data;
using concerts_gate.server.DTOs.Bookings;
using concerts_gate.server.Entities;
using concerts_gate.server.Repositories.Implementations;
using concerts_gate.server.Services.Implementations;
using Xunit;

namespace concerts_gate.tests;

/// <summary>
/// Unit tests for <see cref="BookingService"/> testing anti-overselling, reservation hold TTL, and ticket payment issuance.
/// </summary>
public class BookingServiceTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateBooking_WithValidTickets_ShouldDeductRemainingAndIncreaseReserved()
    {
        // Arrange
        using var context = CreateDbContext();
        var concert = new Concert
        {
            Id = Guid.NewGuid(),
            Title = "Cyber Fest",
            Artist = "Cyber Waves",
            Venue = "Stadium",
            Status = ConcertStatus.Published,
            SaleStartDate = DateTime.UtcNow.AddDays(-1),
            SaleEndDate = DateTime.UtcNow.AddDays(10),
            EventDate = DateTime.UtcNow.AddDays(20)
        };
        var category = new TicketCategory
        {
            Id = Guid.NewGuid(),
            ConcertId = concert.Id,
            Name = "VIP",
            Price = 1000000,
            TotalQuantity = 100,
            RemainingQuantity = 100,
            ReservedQuantity = 0,
            SoldQuantity = 0,
            MaxPerOrder = 4
        };
        concert.TicketCategories.Add(category);

        await context.Concerts.AddAsync(concert);
        await context.SaveChangesAsync();

        var bookingRepo = new BookingRepository(context);
        var concertRepo = new ConcertRepository(context);
        var catRepo = new TicketCategoryRepository(context);
        var voucherRepo = new VoucherRepository(context);
        var auditRepo = new AuditLogRepository(context);
        var voucherService = new VoucherService(voucherRepo, auditRepo);
        var idempRepo = new IdempotencyRepository(context);

        var service = new BookingService(context, bookingRepo, concertRepo, catRepo, voucherRepo, voucherService, idempRepo, auditRepo);

        var userId = Guid.NewGuid();
        var req = new CreateBookingRequestDto
        {
            ConcertId = concert.Id,
            Items = new List<BookingItemRequestDto>
            {
                new BookingItemRequestDto { TicketCategoryId = category.Id, Quantity = 2 }
            }
        };

        // Act
        var result = await service.CreateBookingAsync(req, userId);

        // Assert: Held 2 tickets
        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.PendingPayment);
        result.OriginalAmount.Should().Be(2000000);

        var updatedCategory = await context.TicketCategories.FindAsync(category.Id);
        updatedCategory!.RemainingQuantity.Should().Be(98);
        updatedCategory.ReservedQuantity.Should().Be(2);
    }

    [Fact]
    public async Task CreateBooking_WhenRemainingTicketsInsufficient_ShouldThrowConcurrencyException()
    {
        // Arrange
        using var context = CreateDbContext();
        var concert = new Concert
        {
            Id = Guid.NewGuid(),
            Title = "Cyber Fest",
            Artist = "Cyber Waves",
            Venue = "Stadium",
            Status = ConcertStatus.Published,
            SaleStartDate = DateTime.UtcNow.AddDays(-1),
            SaleEndDate = DateTime.UtcNow.AddDays(10),
            EventDate = DateTime.UtcNow.AddDays(20)
        };
        var category = new TicketCategory
        {
            Id = Guid.NewGuid(),
            ConcertId = concert.Id,
            Name = "VIP",
            Price = 1000000,
            TotalQuantity = 10,
            RemainingQuantity = 1, // only 1 ticket left
            ReservedQuantity = 0,
            SoldQuantity = 9,
            MaxPerOrder = 4
        };
        concert.TicketCategories.Add(category);

        await context.Concerts.AddAsync(concert);
        await context.SaveChangesAsync();

        var bookingRepo = new BookingRepository(context);
        var concertRepo = new ConcertRepository(context);
        var catRepo = new TicketCategoryRepository(context);
        var voucherRepo = new VoucherRepository(context);
        var auditRepo = new AuditLogRepository(context);
        var voucherService = new VoucherService(voucherRepo, auditRepo);
        var idempRepo = new IdempotencyRepository(context);

        var service = new BookingService(context, bookingRepo, concertRepo, catRepo, voucherRepo, voucherService, idempRepo, auditRepo);

        var userId = Guid.NewGuid();
        var req = new CreateBookingRequestDto
        {
            ConcertId = concert.Id,
            Items = new List<BookingItemRequestDto>
            {
                new BookingItemRequestDto { TicketCategoryId = category.Id, Quantity = 2 } // requesting 2 tickets when only 1 is available
            }
        };

        // Act & Assert (Anti-Overselling Concurrency Protection)
        var act = async () => await service.CreateBookingAsync(req, userId);
        await act.Should().ThrowAsync<ConcurrencyException>()
            .WithMessage("*insufficient available seats*");
    }

    [Fact]
    public async Task ProcessPayment_ShouldConfirmBooking_AndGenerateTicketsWithQrCode()
    {
        // Arrange
        using var context = CreateDbContext();
        var userId = Guid.NewGuid();
        var concert = new Concert
        {
            Id = Guid.NewGuid(),
            Title = "Cyber Fest",
            Artist = "Cyber Waves",
            Venue = "Stadium",
            Status = ConcertStatus.Published,
            SaleStartDate = DateTime.UtcNow.AddDays(-1),
            SaleEndDate = DateTime.UtcNow.AddDays(10),
            EventDate = DateTime.UtcNow.AddDays(20)
        };
        var category = new TicketCategory
        {
            Id = Guid.NewGuid(),
            ConcertId = concert.Id,
            Name = "VIP",
            Price = 500000,
            TotalQuantity = 50,
            RemainingQuantity = 48,
            ReservedQuantity = 2,
            SoldQuantity = 0,
            MaxPerOrder = 4
        };
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "CG-TEST-1234",
            UserId = userId,
            ConcertId = concert.Id,
            OriginalAmount = 1000000,
            FinalAmount = 1000000,
            Status = BookingStatus.PendingPayment,
            ReservationExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow,
            BookingItems = new List<BookingItem>
            {
                new BookingItem { Id = Guid.NewGuid(), TicketCategoryId = category.Id, Quantity = 2, UnitPrice = 500000, TicketCategory = category }
            }
        };

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "testuser@gmail.com",
            Email = "testuser@gmail.com",
            FullName = "Test User"
        };
        await context.Users.AddAsync(user);
        await context.Concerts.AddAsync(concert);
        await context.TicketCategories.AddAsync(category);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        var bookingRepo = new BookingRepository(context);
        var concertRepo = new ConcertRepository(context);
        var catRepo = new TicketCategoryRepository(context);
        var voucherRepo = new VoucherRepository(context);
        var auditRepo = new AuditLogRepository(context);
        var voucherService = new VoucherService(voucherRepo, auditRepo);
        var idempRepo = new IdempotencyRepository(context);

        var service = new BookingService(context, bookingRepo, concertRepo, catRepo, voucherRepo, voucherService, idempRepo, auditRepo);

        // Act: Process simulated payment
        var result = await service.ProcessPaymentAsync(booking.Id, userId, new PaymentSimulationRequestDto { PaymentMethod = "VNPAY" });

        // Assert
        result.Status.Should().Be(BookingStatus.Confirmed);
        result.PaidAt.Should().NotBeNull();
        result.Tickets.Should().HaveCount(2);
        result.Tickets.First().QrCodePayload.Should().Contain("CG-PASS:");

        var updatedCategory = await context.TicketCategories.FindAsync(category.Id);
        updatedCategory!.ReservedQuantity.Should().Be(0);
        updatedCategory.SoldQuantity.Should().Be(2);
    }
}
