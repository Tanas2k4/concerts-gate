# Unit & Concurrency Test Suites - Guidelines

This directory contains the **Unit Tests** and **Concurrency Contention Tests** validating domain logic, flash sale guarantees, and background worker behavior.

---

## 1. Technologies & Frameworks

- **xUnit**: Standard .NET test execution framework.
- **FluentAssertions**: Readable, expressive assertion syntax.
- **Moq**: Mocking framework for isolating dependencies.
- **Microsoft.EntityFrameworkCore.InMemory**: Isolated in-memory database instances for each test, ensuring zero side-effects on physical database instances.

---

## 2. Test Naming Convention

All test methods follow the 3-part naming structure:
$$\textbf{[MethodName]\_[StateUnderTest]\_[ExpectedBehavior]}$$

| Example Test Method | Intent |
| :--- | :--- |
| `CreateBooking_WhenSufficientInventory_ShouldSucceedAndReserveTickets` | Verifies successful reservation and atomic hold counter increments when stock is sufficient. |
| `CreateBooking_WhenInsufficientInventory_ShouldThrowBadRequestException` | Verifies rejection when requested count exceeds available inventory. |
| `CreateBooking_WithDuplicateIdempotencyKey_ShouldReturnCachedResult` | Verifies idempotency cache returns original response without decrementing inventory twice. |
| `ValidateVoucher_WhenUserExceedsMaxUsage_ShouldThrowVoucherException` | Verifies rejection when user has exhausted per-user voucher allowance. |

---

## 3. Standard Test Pattern (Arrange - Act - Assert)

```csharp
[Fact]
public async Task ValidateVoucher_WithValidPercentageCode_ShouldCalculateCorrectDiscount()
{
    // 1. Arrange: Setup in-memory context and seed voucher entity
    var dbContext = CreateInMemoryDbContext();
    var voucher = new Voucher
    {
        Id = Guid.NewGuid(),
        Code = "SALE20",
        DiscountType = DiscountType.Percentage,
        DiscountValue = 20,
        MaxDiscountAmount = 500000,
        MinOrderAmount = 100000,
        IsActive = true,
        ValidFrom = DateTime.UtcNow.AddDays(-1),
        ValidTo = DateTime.UtcNow.AddDays(5),
        MaxUsageCount = 100,
        MaxUsagePerUser = 2
    };
    await dbContext.Vouchers.AddAsync(voucher);
    await dbContext.SaveChangesAsync();

    var voucherService = CreateVoucherService(dbContext);

    // 2. Act: Execute business method
    var result = await voucherService.ValidateAndCalculateDiscountAsync("SALE20", Guid.NewGuid(), 1000000);

    // 3. Assert: Validate results with FluentAssertions
    result.Should().NotBeNull();
    result.DiscountAmount.Should().Be(200000); // 20% of 1,000,000 = 200,000 VND
}
```

---

## 4. Test Execution Commands

Run all tests:
```bash
dotnet test
```

Run with detailed verbose logging:
```bash
dotnet test --logger "console;verbosity=detailed"
```
