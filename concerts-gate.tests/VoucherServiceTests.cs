using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using concerts_gate.server.Common.Enums;
using concerts_gate.server.Common.Exceptions;
using concerts_gate.server.Data;
using concerts_gate.server.DTOs.Vouchers;
using concerts_gate.server.Entities;
using concerts_gate.server.Repositories.Implementations;
using concerts_gate.server.Services.Implementations;
using Xunit;

namespace concerts_gate.tests;

/// <summary>
/// Unit tests for <see cref="VoucherService"/> testing discount calculation and anti-abuse limits.
/// </summary>
public class VoucherServiceTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ValidateVoucher_WithValidPercentageCode_ShouldCalculateDiscountCorrectly()
    {
        // Arrange
        using var context = CreateDbContext();
        var voucher = new Voucher
        {
            Id = Guid.NewGuid(),
            Code = "SALE20",
            DiscountType = DiscountType.Percentage,
            DiscountValue = 20,
            MaxDiscountAmount = 200000,
            MinOrderAmount = 500000,
            MaxUsageCount = 100,
            CurrentUsageCount = 10,
            MaxUsagePerUser = 1,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(5),
            IsActive = true
        };
        await context.Vouchers.AddAsync(voucher);
        await context.SaveChangesAsync();

        var voucherRepo = new VoucherRepository(context);
        var auditRepo = new AuditLogRepository(context);
        var service = new VoucherService(voucherRepo, auditRepo);

        var userId = Guid.NewGuid();

        // Act
        var result = await service.ValidateAndCalculateDiscountAsync("SALE20", userId, 800000);

        // Assert: 800,000 * 20% = 160,000 (below cap of 200,000)
        result.Should().NotBeNull();
        result.DiscountAmount.Should().Be(160000);
        result.FinalAmount.Should().Be(640000);
    }

    [Fact]
    public async Task ValidateVoucher_WhenUserExceedsPerUserLimit_ShouldThrowVoucherException()
    {
        // Arrange
        using var context = CreateDbContext();
        var userId = Guid.NewGuid();
        var voucher = new Voucher
        {
            Id = Guid.NewGuid(),
            Code = "ONETIME",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 50000,
            MinOrderAmount = 100000,
            MaxUsageCount = 100,
            CurrentUsageCount = 5,
            MaxUsagePerUser = 1,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(5),
            IsActive = true
        };
        var existingUsage = new VoucherUsage
        {
            Id = Guid.NewGuid(),
            VoucherId = voucher.Id,
            UserId = userId,
            BookingId = Guid.NewGuid(),
            DiscountApplied = 50000,
            UsedAt = DateTime.UtcNow
        };
        await context.Vouchers.AddAsync(voucher);
        await context.VoucherUsages.AddAsync(existingUsage);
        await context.SaveChangesAsync();

        var voucherRepo = new VoucherRepository(context);
        var auditRepo = new AuditLogRepository(context);
        var service = new VoucherService(voucherRepo, auditRepo);

        // Act & Assert (User redemption limit enforcement)
        var act = async () => await service.ValidateAndCalculateDiscountAsync("ONETIME", userId, 200000);
        await act.Should().ThrowAsync<VoucherException>()
            .WithMessage("*maximum allowed limit of 1*");
    }

    [Fact]
    public async Task ValidateVoucher_WhenExpired_ShouldThrowVoucherException()
    {
        // Arrange
        using var context = CreateDbContext();
        var voucher = new Voucher
        {
            Id = Guid.NewGuid(),
            Code = "EXPIRED",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 50000,
            MinOrderAmount = 100000,
            MaxUsageCount = 100,
            CurrentUsageCount = 0,
            MaxUsagePerUser = 1,
            ValidFrom = DateTime.UtcNow.AddDays(-10),
            ValidTo = DateTime.UtcNow.AddDays(-1), // already expired
            IsActive = true
        };
        await context.Vouchers.AddAsync(voucher);
        await context.SaveChangesAsync();

        var voucherRepo = new VoucherRepository(context);
        var auditRepo = new AuditLogRepository(context);
        var service = new VoucherService(voucherRepo, auditRepo);

        // Act & Assert
        var act = async () => await service.ValidateAndCalculateDiscountAsync("EXPIRED", Guid.NewGuid(), 200000);
        await act.Should().ThrowAsync<VoucherException>()
            .WithMessage("*has expired*");
    }
}
