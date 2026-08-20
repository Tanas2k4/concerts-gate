# Data Transfer Objects (DTOs) - Coding Guidelines & Conventions

This directory contains all **Data Transfer Objects (DTOs)** used to transport data across API boundaries between Client and Server, organized by business module.

---

## 1. Directory Structure

```
DTOs/
├── Auth/           # Registration, login, user profile
├── Concerts/       # Concert details, summaries, management DTOs
├── Tickets/        # Category definitions, ticket models, QR payloads
├── Bookings/       # Reservation requests, responses, payment payloads
├── Vouchers/       # Validation requests/responses, campaign DTOs
└── Operations/     # Dashboard statistics, reconciliation reports, status updates
```

---

## 2. Naming Standards & Suffixes

Each DTO is defined in an **isolated file** with an explicit suffix indicating its intent:

| Suffix | Purpose | Example |
| :--- | :--- | :--- |
| **`...RequestDto`** | Input payload sent in the HTTP request body | `CreateBookingRequestDto.cs`, `LoginRequestDto.cs` |
| **`...ResponseDto`** | Detailed payload returned on operation success | `BookingResponseDto.cs`, `AuthResponseDto.cs` |
| **`...SummaryDto`** | Lightweight model optimized for paginated lists | `ConcertSummaryDto.cs`, `BookingSummaryDto.cs` |

---

## 3. DataAnnotations & Validation Rules

All properties in **Request DTOs** must declare DataAnnotations to reject malformed data at the controller boundary:

| Annotation | Validation Rule | Example Error Message |
| :--- | :--- | :--- |
| `[Required]` | Field cannot be null or empty | `ErrorMessage = "ConcertId is required."` |
| `[StringLength(max, MinimumLength = min)]` | String length constraints | `ErrorMessage = "Password must be between 6 and 100 characters."` |
| `[Range(min, max)]` | Numeric bounds | `ErrorMessage = "Ticket quantity must be between 1 and 4."` |
| `[EmailAddress]` | Valid RFC email format | `ErrorMessage = "Invalid email format."` |
| `[MinLength(1)]` | Collection has at least N items | `ErrorMessage = "At least one ticket category must be selected."` |

---

## 4. Standard DTO File Template

```csharp
using System.ComponentModel.DataAnnotations;

namespace concerts_gate.server.DTOs.Bookings;

/// <summary>
/// Payload for simulating online payment on a ticket reservation.
/// </summary>
public class PaymentSimulationRequestDto
{
    /// <summary>
    /// Payment method identifier (VNPAY, MOMO, CREDIT_CARD).
    /// </summary>
    [Required(ErrorMessage = "Payment method is required.")]
    [MaxLength(50, ErrorMessage = "Payment method name cannot exceed 50 characters.")]
    public string PaymentMethod { get; set; } = "VNPAY";

    /// <summary>
    /// External transaction reference code.
    /// </summary>
    [MaxLength(100, ErrorMessage = "Transaction reference cannot exceed 100 characters.")]
    public string? TransactionReference { get; set; }
}
```
