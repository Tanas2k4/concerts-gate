# Repository Layer - Coding Guidelines & Conventions

This directory contains the **Data Access Layer**, encapsulating Entity Framework Core queries and abstracting direct database operations away from the service layer.

---

## 1. Structure

```
Repositories/
├── Interfaces/                 # Data access contracts
│   ├── IBaseRepository.cs      # Generic CRUD operations
│   ├── IBookingRepository.cs   # Specialized booking queries
│   ├── IConcertRepository.cs   # Specialized concert queries
│   ├── IVoucherRepository.cs   # Specialized voucher queries
│   └── ...
└── Implementations/            # EF Core implementations
    ├── BaseRepository.cs       # Generic CRUD implementation
    ├── BookingRepository.cs
    ├── ConcertRepository.cs
    └── ...
```

---

## 2. Query Method Naming Standards

| Query Purpose | Standard Method Name | Example |
| :--- | :--- | :--- |
| **Get by Primary Key** | `GetByIdAsync` | `Task<T?> GetByIdAsync(Guid id, ...)` |
| **Get detailed with relations (`Include`)** | `GetDetailedByIdAsync` | `Task<Booking?> GetDetailedByIdAsync(Guid id, ...)` |
| **Find by unique code** | `GetByCodeAsync` | `Task<Voucher?> GetByCodeAsync(string code, ...)` |
| **Count by condition** | `CountBy...Async` | `Task<int> GetUserUsageCountAsync(Guid voucherId, Guid userId, ...)` |
| **Filtered list retrieval** | `Get[Condition]Async` | `Task<List<Booking>> GetExpiredPendingBookingsAsync(DateTime now, ...)` |
| **Batch addition** | `AddRangeAsync` | `Task AddTicketsAsync(IEnumerable<Ticket> tickets, ...)` |

---

## 3. Performance & EF Core Best Practices

### 1. Always enforce `.AsNoTracking()` on read-only queries:
When querying data solely for reading or serialization, use `.AsNoTracking()` to eliminate EF Core Change Tracker memory overhead:
```csharp
public async Task<List<Concert>> GetActiveConcertsAsync(CancellationToken cancellationToken = default)
{
    return await _context.Concerts
        .Include(c => c.TicketCategories)
        .Where(c => c.Status == ConcertStatus.Published)
        .AsNoTracking()
        .ToListAsync(cancellationToken);
}
```

### 2. Selective Eager Loading:
Only include related entities (`.Include()`, `.ThenInclude()`) that are strictly required by the operation, avoiding large unused columns.

### 3. Pagination Support:
Queries returning variable collections must support pagination via `Skip` and `Take`:
```csharp
var items = await query
    .Skip((pageIndex - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);
```

### 4. Propagate `CancellationToken`:
All asynchronous EF Core operations (`ToListAsync`, `FirstOrDefaultAsync`, `CountAsync`) must accept and pass the `CancellationToken` to cancel SQL queries if client connections abort.
