namespace concerts_gate.server.Common.Models;

/// <summary>
/// Standard API response wrapper across the system.
/// </summary>
/// <typeparam name="T">Payload data type.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the request was processed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// User-friendly response message or error summary.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Response payload data.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Detailed list of error messages (if any).
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// UTC timestamp of response generation.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="data">Response payload data.</param>
    /// <param name="message">Success message.</param>
    /// <returns>A successful <see cref="ApiResponse{T}"/> object.</returns>
    public static ApiResponse<T> Ok(T data, string message = "Operation completed successfully.")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Creates a failure response.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="errors">Detailed list of errors.</param>
    /// <returns>A failed <see cref="ApiResponse{T}"/> object.</returns>
    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors
        };
    }
}
