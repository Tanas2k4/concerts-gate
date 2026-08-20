# Domain Entities Layer - Coding Guidelines & Conventions

This directory contains the 11 **Domain Entity Models** mapped directly to Microsoft SQL Server tables via Entity Framework Core 10.

---

## 1. Domain Entities Directory

| Entity File | SQL Table Name | Description |
| :--- | :--- | :--- |
| **`ApplicationUser.cs`** | `AspNetUsers` | User identity entity inheriting from `IdentityUser<Guid>`. |
| **`ApplicationRole.cs`** | `AspNetRoles` | Identity role entity (`Customer`, `Operator`, `Admin`). |
| **`Concert.cs`** | `Concerts` | Concert event data, sale windows, flash sale flags. |
| **`TicketCategory.cs`** | `TicketCategories` | Category tiers (VIP, GA), inventory counters, and `RowVersion`. |
| **`Booking.cs`** | `Bookings` | Order reservations, reference codes, state machine, TTL. |
| **`BookingItem.cs`** | `BookingItems` | Line items detailing quantity and category per booking. |
| **`Ticket.cs`** | `Tickets` | Individual post-payment digital tickets with unique QR payloads. |
| **`Voucher.cs`** | `Vouchers` | Promotional campaign rules, discount values, and usage caps. |
| **`VoucherUsage.cs`** | `VoucherUsages` | Audit tracking of voucher applications per user. |
| **`IdempotencyRecord.cs`** | `IdempotencyRecords` | Cached execution records keyed by client idempotency tokens. |
| **`AuditLog.cs`** | `AuditLogs` | Structured audit trail of operator/admin manual interventions. |

---

## 2. Modeling Standards

### 1. GUID Primary Keys:
All entities declare a `Guid Id` primary key to support distributed, collision-resistant identifier generation:
```csharp
[Key]
public Guid Id { get; set; } = Guid.NewGuid();
```

### 2. Concurrency Token (`RowVersion`):
Entities subject to high-throughput concurrent writes (`TicketCategory`) declare a `[Timestamp] byte[] RowVersion`:
```csharp
/// <summary>
/// Concurrency token for Optimistic Concurrency Control (OCC) preventing ticket overselling.
/// </summary>
[Timestamp]
public byte[] RowVersion { get; set; } = Array.Empty<byte>();
```

### 3. Inventory Conservation Invariant:
The `TicketCategory` table must satisfy the mathematical invariant at all times:
$$\text{TotalQuantity} = \text{RemainingQuantity} + \text{ReservedQuantity} + \text{SoldQuantity}$$

### 4. Strongly-Typed Enums:
State properties must use strongly-typed enums rather than raw strings (`BookingStatus`, `ConcertStatus`, `TicketStatus`, `DiscountType`, `UserRole`).
