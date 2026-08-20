using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.BackgroundTasks;

/// <summary>
/// Background Hosted Service running periodically to scan and release unpaid booking reservation holds (PendingPayment TTL).
/// </summary>
/// <remarks>
/// Ensures reserved seats held longer than 10 minutes without payment are returned to available inventory (RemainingQuantity),
/// allowing other customers to purchase tickets during high-demand Flash Sales.
/// </remarks>
public class BookingExpirationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingExpirationWorker> _logger;

    /// <summary>
    /// Database scan interval (default every 30 seconds).
    /// </summary>
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Initializes a new instance of <see cref="BookingExpirationWorker"/>.
    /// </summary>
    /// <param name="serviceProvider">Dependency injection service provider.</param>
    /// <param name="logger">Logger instance.</param>
    public BookingExpirationWorker(IServiceProvider serviceProvider, ILogger<BookingExpirationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BookingExpirationWorker started. Scan interval: {Interval} seconds.", _checkInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                var releasedCount = await bookingService.ReleaseExpiredBookingsAsync(stoppingToken);
                if (releasedCount > 0)
                {
                    _logger.LogInformation("[Auto-Release] Successfully released {Count} expired booking reservations and returned tickets to inventory.", releasedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while scanning and releasing expired bookings.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
