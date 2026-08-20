namespace concerts_gate.server.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found in the system.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="NotFoundException"/> with an error message.
    /// </summary>
    /// <param name="message">Detailed message about the missing resource.</param>
    public NotFoundException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when request data is invalid or violates business rules.
/// </summary>
public class BadRequestException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="BadRequestException"/> with an error message.
    /// </summary>
    /// <param name="message">Business error message.</param>
    public BadRequestException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when a concurrency conflict occurs (e.g. flash sale ticket inventory race conditions).
/// </summary>
public class ConcurrencyException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="ConcurrencyException"/> with an error message.
    /// </summary>
    /// <param name="message">Concurrency conflict message.</param>
    public ConcurrencyException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when a voucher is invalid, expired, exhausted, or misused.
/// </summary>
public class VoucherException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="VoucherException"/> with an error message.
    /// </summary>
    /// <param name="message">Voucher error message.</param>
    public VoucherException(string message) : base(message)
    {
    }
}
