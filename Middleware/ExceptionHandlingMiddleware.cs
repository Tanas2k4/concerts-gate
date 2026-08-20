using System.Net;
using System.Text.Json;
using concerts_gate.server.Common.Exceptions;
using concerts_gate.server.Common.Models;

namespace concerts_gate.server.Middleware;

/// <summary>
/// Global exception handling middleware formatting errors into standard <see cref="ApiResponse{T}"/> responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ExceptionHandlingMiddleware"/>.
    /// </summary>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Processes the HttpContext and catches unhandled exceptions thrown across controllers and services.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message),
            BadRequestException badRequest => (HttpStatusCode.BadRequest, badRequest.Message),
            ConcurrencyException concurrency => (HttpStatusCode.Conflict, concurrency.Message),
            VoucherException voucher => (HttpStatusCode.UnprocessableEntity, voucher.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "You do not have permission to access this resource."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected internal server error occurred. Please try again later.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
