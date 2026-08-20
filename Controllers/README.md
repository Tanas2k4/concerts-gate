# Controller Layer - Coding Guidelines & Conventions

This directory contains the **API Controllers** that handle incoming HTTP requests, enforce role-based access control (RBAC), delegate business workflows to the Service Layer, and return standardized JSON responses.

---

## 1. Naming Conventions & Structure

| Component | Convention | Project Example |
| :--- | :--- | :--- |
| **File & Class Name** | PascalCase, suffixed with `Controller` | `BookingsController.cs`, `AdminConcertsController.cs` |
| **Action Method** | PascalCase describing business action | `CreateBooking`, `GetConcerts`, `ProcessPayment`, `FlagSuspicious` |
| **Route Parameter** | camelCase | `[FromRoute] Guid id`, `[FromRoute] Guid categoryId` |
| **Query Parameter** | camelCase | `[FromQuery] string? search`, `[FromQuery] int pageIndex = 1` |
| **Header Parameter** | PascalCase in code, Kebab-Case in HTTP | `[FromHeader(Name = "X-Idempotency-Key")] string? idempotencyKey` |
| **Body Parameter** | camelCase, typed as a Request DTO | `[FromBody] CreateBookingRequestDto request` |
| **CancellationToken** | camelCase, placed as the final parameter | `CancellationToken cancellationToken = default` |

---

## 2. RESTful Routing & HTTP Verb Standards

| Action | HTTP Verb | Route Template | Expected Status Code |
| :--- | :--- | :--- | :--- |
| **List Resources** | `GET` | `/api/concerts`, `/api/bookings/my-bookings` | `200 OK` |
| **Get by ID** | `GET` | `/api/concerts/{id:guid}` | `200 OK`, `404 NotFound` |
| **Create Resource** | `POST` | `/api/bookings`, `/api/auth/login` | `200 OK` / `201 Created` |
| **Execute Action** | `POST` | `/api/bookings/{id:guid}/pay`, `/api/vouchers/validate` | `200 OK`, `400 BadRequest` |
| **Update Resource** | `PUT` | `/api/admin/bookings/{id:guid}/status` | `200 OK`, `400 BadRequest` |
| **Flag / Toggle** | `PUT` / `PATCH` | `/api/admin/bookings/{id:guid}/flag-suspicious` | `200 OK` |
| **Delete / Cancel** | `DELETE` | `/api/bookings/{id:guid}` | `200 OK` / `204 NoContent` |

---

## 3. Standard Controller Action Template

Every controller action must declare:
1. **HTTP Method Attribute**: `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`.
2. **Access Control (RBAC)**: `[Authorize]`, `[Authorize(Roles = AppConstants.Roles.InternalOperations)]`, or `[AllowAnonymous]`.
3. **XML Documentation Comments**: `<summary>`, `<param>`, `<response code="...">`.
4. **`[ProducesResponseType]`**: Enumerate all possible response types and status codes.
5. **Standard Response Wrapper**: Responses must be wrapped in `ApiResponse<T>.Ok(data, message)`.

```csharp
/// <summary>
/// Creates a new ticket reservation with a 10-minute payment hold (TTL).
/// </summary>
/// <param name="request">Concert selection, category, quantity, and optional voucher code.</param>
/// <param name="idempotencyKey">Client idempotency key preventing duplicate orders upon retry.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <response code="200">Reservation created successfully; returns booking details and hold expiration timestamp.</response>
/// <response code="400">Invalid payload, concert not open for sale, or purchase quantity limits exceeded.</response>
/// <response code="401">Unauthenticated or expired JWT token.</response>
/// <response code="409">Ticket inventory contention or sold out during flash sale.</response>
[HttpPost]
[Authorize]
[ProducesResponseType(typeof(ApiResponse<BookingResponseDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
public async Task<IActionResult> CreateBooking(
    [FromBody] CreateBookingRequestDto request,
    [FromHeader(Name = "X-Idempotency-Key")] string? idempotencyKey,
    CancellationToken cancellationToken)
{
    // Extract authenticated user ID from JWT Claims
    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Delegate all business rules to the service layer
    var result = await _bookingService.CreateBookingAsync(request, userId, idempotencyKey, cancellationToken);

    // Return standardized response envelope
    return Ok(ApiResponse<BookingResponseDto>.Ok(result, "Ticket reservation created successfully."));
}
```

---

## 4. Anti-Patterns to Avoid
- **Do not write business logic inside controllers**: Database operations and pricing logic belong strictly in services.
- **Do not return raw unwrapped objects**: Always return `ApiResponse<T>.Ok(...)`.
- **Do not swallow exceptions with try-catch blocks**: Let `ExceptionHandlingMiddleware` catch domain exceptions (`NotFoundException`, `BadRequestException`, `ConcurrencyException`) and map them to HTTP status codes automatically.
