# Service Layer - Coding Guidelines & Conventions

This directory contains the **Core Business Logic Layer**, responsible for inventory management, flash sale concurrency control (OCC), idempotency enforcement, voucher anti-abuse rules, and payment processing.

---

## 1. Interface and Implementation Separation

Following the Dependency Inversion Principle:
- **Interfaces**: Located in `Services/Interfaces/` (e.g., `IBookingService.cs`, `IVoucherService.cs`).
- **Implementations**: Located in `Services/Implementations/` (e.g., `BookingService.cs`, `VoucherService.cs`).

---

## 2. Naming Conventions & Parameters

| Component | Convention | Project Example |
| :--- | :--- | :--- |
| **Interface** | Prefixed with `I` + PascalCase + `Service` | `IBookingService`, `IVoucherService` |
| **Implementation Class** | PascalCase + `Service` | `BookingService`, `VoucherService` |
| **Asynchronous Method** | PascalCase, suffixed with `Async` | `CreateBookingAsync`, `ProcessPaymentAsync` |
| **Identifier Parameter** | camelCase, suffixed with `Id` | `Guid userId`, `Guid concertId`, `Guid bookingId` |
| **DTO Parameter** | camelCase | `CreateBookingRequestDto request`, `PaymentSimulationRequestDto request` |
| **CancellationToken** | camelCase, default value assigned | `CancellationToken cancellationToken = default` |

---

## 3. Custom Exception Handling Standards

Services throw domain-specific custom exceptions defined in `Common/Exceptions/`:

| Exception Class | Usage Scenario | Mapped HTTP Status |
| :--- | :--- | :--- |
| **`NotFoundException`** | Entity not found in database | `404 Not Found` |
| **`BadRequestException`** | Domain validation failure (exceeded limits, invalid state) | `400 Bad Request` |
| **`VoucherException`** | Expired, inactive, or exhausted voucher usage limits | `422 Unprocessable Entity` |
| **`ConcurrencyException`** | `RowVersion` mismatch during high-concurrency contention | `409 Conflict` |
| **`UnauthorizedException`** | User attempting to access/modify another user's order | `403 Forbidden` |

---

## 4. Transaction Management & Concurrency Implementation

Multi-table write workflows (Reservation -> Hold inventory -> Record voucher usage -> Record idempotency) must execute within `ExecutionStrategy` and `BeginTransactionAsync`:

```csharp
public async Task<BookingResponseDto> CreateBookingAsync(
    CreateBookingRequestDto request,
    Guid userId,
    string? idempotencyKey,
    CancellationToken cancellationToken = default)
{
    // 1. Check idempotency cache
    if (!string.IsNullOrWhiteSpace(idempotencyKey))
    {
        var existingRecord = await _idempotencyRepository.GetByKeyAsync(idempotencyKey.Trim(), userId, cancellationToken);
        if (existingRecord != null)
        {
            return JsonSerializer.Deserialize<BookingResponseDto>(existingRecord.ResponseBody)!;
        }
    }

    // 2. Wrap operations in execution strategy to handle transient connection retries
    var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

    return await executionStrategy.ExecuteAsync(async () =>
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // 3. Atomically check and reserve inventory
            var category = await _categoryRepository.GetByIdAsync(request.Items[0].TicketCategoryId, cancellationToken);
            if (category.RemainingQuantity < request.Items[0].Quantity)
            {
                throw new BadRequestException("Insufficient ticket inventory remaining.");
            }

            category.RemainingQuantity -= request.Items[0].Quantity;
            category.ReservedQuantity += request.Items[0].Quantity;
            _categoryRepository.Update(category);

            // 4. Create and persist Booking entity
            var booking = new Booking { /* ... */ };
            await _bookingRepository.AddAsync(booking, cancellationToken);

            // 5. Commit database changes
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return MapToResponseDto(booking, concert);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            // Catch RowVersion version conflict under flash sale load
            throw new ConcurrencyException("Ticket was reserved by another user in the same millisecond. Please try again.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    });
}
```

---

## 5. Standard XML Documentation for Service Interfaces

Every interface method in `Services/Interfaces/` must include comprehensive XML comments:

```csharp
/// <summary>
/// Creates a new ticket reservation and holds inventory for 10 minutes (with flash sale concurrency control).
/// </summary>
/// <param name="request">Concert selection, categories, quantities, and promo code.</param>
/// <param name="userId">Authenticated user identifier.</param>
/// <param name="idempotencyKey">Client idempotency token.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>Created booking details with expiration hold timestamp.</returns>
/// <exception cref="NotFoundException">Thrown when concert or ticket category is not found.</exception>
/// <exception cref="BadRequestException">Thrown when purchase limits are exceeded or inventory is insufficient.</exception>
/// <exception cref="ConcurrencyException">Thrown when optimistic concurrency token conflict occurs.</exception>
Task<BookingResponseDto> CreateBookingAsync(
    CreateBookingRequestDto request, 
    Guid userId, 
    string? idempotencyKey, 
    CancellationToken cancellationToken = default);
```
