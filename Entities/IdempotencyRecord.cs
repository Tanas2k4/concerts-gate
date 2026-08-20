namespace concerts_gate.server.Entities;

/// <summary>
/// Entity storing the processing status and cached response of requests sent with an Idempotency Key header (prevents duplicate bookings during network retries).
/// </summary>
public class IdempotencyRecord
{
    /// <summary>
    /// Unique identifier of the record (GUID).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Client-supplied idempotency key (e.g. client-generated UUID).
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the user who initiated the request.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Target API route path (e.g. "/api/bookings").
    /// </summary>
    public string RequestPath { get; set; } = string.Empty;

    /// <summary>
    /// HTTP status code produced upon initial successful/failed processing (200, 201, 400, etc.).
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Cached JSON response body returned to duplicate incoming requests.
    /// </summary>
    public string ResponseBody { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the request was first accepted (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Expiration timestamp of the cached idempotency record (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
