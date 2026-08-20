namespace concerts_gate.server.Common.Constants;

/// <summary>
/// Contains system-wide constants (Roles, Security policies, Reservation TTL).
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// User role constants across the system.
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// System Administrator role.
        /// </summary>
        public const string Admin = "Admin";

        /// <summary>
        /// Event Operator role.
        /// </summary>
        public const string Operator = "Operator";

        /// <summary>
        /// Standard Customer role.
        /// </summary>
        public const string Customer = "Customer";

        /// <summary>
        /// Roles allowed to access internal operational endpoints.
        /// </summary>
        public const string InternalOperations = "Admin,Operator";
    }

    /// <summary>
    /// Booking business logic and flash sale configuration constants.
    /// </summary>
    public static class BookingConfig
    {
        /// <summary>
        /// Temporary reservation hold time (minutes) before payment expiration. Default is 10 minutes.
        /// </summary>
        public const int ReservationTtlMinutes = 10;

        /// <summary>
        /// Maximum number of tickets an account can reserve per booking order.
        /// </summary>
        public const int MaxTicketsPerOrder = 4;

        /// <summary>
        /// HTTP header name carrying the Idempotency Key to prevent duplicate orders on network retries.
        /// </summary>
        public const string IdempotencyHeaderName = "X-Idempotency-Key";

        /// <summary>
        /// Retention period for idempotency records in hours.
        /// </summary>
        public const int IdempotencyTtlHours = 24;
    }
}
