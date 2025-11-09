namespace PaymentGateway.Api.Models;

/// <summary>
/// The possible statuses for a payment.
/// </summary>
public enum PaymentStatus
{
    Authorized,
    Declined,
    Rejected, 
    Pending,
    InternalError
}