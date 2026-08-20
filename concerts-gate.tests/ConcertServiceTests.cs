using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using concerts_gate.server.Common.Enums;
using concerts_gate.server.Common.Exceptions;
using concerts_gate.server.Data;
using concerts_gate.server.DTOs.Concerts;
using concerts_gate.server.DTOs.Tickets;
using concerts_gate.server.Entities;
using concerts_gate.server.Repositories.Implementations;
using concerts_gate.server.Services.Implementations;
using Xunit;

namespace concerts_gate.tests;

/// <summary>
/// Unit tests for <see cref="ConcertService"/> testing concert management and category inventory adjustments.
/// </summary>
public class ConcertServiceTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateConcert_WithCategories_ShouldPersistSuccessfully()
    {
        // Arrange
        using var context = CreateDbContext();
        var concertRepo = new ConcertRepository(context);
        var catRepo = new TicketCategoryRepository(context);
        var auditRepo = new AuditLogRepository(context);
        var service = new ConcertService(concertRepo, catRepo, auditRepo);

        var operatorId = Guid.NewGuid();
        var dto = new CreateConcertDto
        {
            Title = "Cyber Wave Live 2026",
            Artist = "Cyber Wave",
            Description = "Live EDM concert",
            Venue = "My Dinh Stadium",
            BannerUrl = "https://example.com/banner.jpg",
            Genre = "EDM",
            EventDate = DateTime.UtcNow.AddDays(30),
            SaleStartDate = DateTime.UtcNow.AddDays(-1),
            SaleEndDate = DateTime.UtcNow.AddDays(25),
            IsFlashSale = true,
            Categories = new List<CreateTicketCategoryDto>
            {
                new CreateTicketCategoryDto
                {
                    Name = "VIP Early Bird",
                    Description = "Fanzone pass",
                    Price = 1200000,
                    TotalQuantity = 100,
                    MaxPerOrder = 2
                }
            }
        };

        // Act
        var result = await service.CreateConcertAsync(dto, operatorId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Cyber Wave Live 2026");
        result.Categories.Should().HaveCount(1);
        result.Categories.First().RemainingQuantity.Should().Be(100);
    }

    [Fact]
    public async Task UpdateInventory_WhenNewTotalLessThanSoldAndReserved_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = CreateDbContext();
        var category = new TicketCategory
        {
            Id = Guid.NewGuid(),
            ConcertId = Guid.NewGuid(),
            Name = "VIP",
            Description = "VIP Area",
            Price = 1000000,
            TotalQuantity = 100,
            RemainingQuantity = 20,
            ReservedQuantity = 30, // 30 on hold
            SoldQuantity = 50,     // 50 sold (total committed: 80)
            MaxPerOrder = 4
        };
        await context.TicketCategories.AddAsync(category);
        await context.SaveChangesAsync();

        var concertRepo = new ConcertRepository(context);
        var catRepo = new TicketCategoryRepository(context);
        var auditRepo = new AuditLogRepository(context);
        var service = new ConcertService(concertRepo, catRepo, auditRepo);

        var operatorId = Guid.NewGuid();

        // Act & Assert: Attempting to reduce total tickets to 70 (less than 80 committed tickets)
        var act = async () => await service.UpdateCategoryInventoryAsync(category.Id, new UpdateInventoryDto { NewTotalQuantity = 70 }, operatorId);
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Cannot reduce total tickets*");
    }
}
