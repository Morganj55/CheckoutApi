namespace PaymentGateway.Api.Models.Responses;

/// <summary>
/// Represents the response model returned to the client when retrieving the details of a specific, 
/// previously processed payment.
/// </summary>
public class GetPaymentResponse
{
    /// <summary>
    /// Gets or sets the unique identifier (GUID) of the processed payment transaction.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the final or current status of the payment (e.g., Authorized, Failed).
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the last four digits of the card number used for the transaction.
    /// </summary>
    public string CardNumberLastFour { get; set; }

    /// <summary>
    /// Gets or sets the expiry month of the card.
    /// </summary>
    public int ExpiryMonth { get; set; }

    /// <summary>
    /// Gets or sets the expiry year of the card.
    /// </summary>
    public int ExpiryYear { get; set; }

    /// <summary>
    /// Gets or sets the ISO 4217 currency code of the transaction (e.g., "GBP").
    /// </summary>
    public string Currency { get; set; }

    /// <summary>
    /// Gets or sets the monetary amount of the payment, typically in the smallest currency unit (e.g., cents or pence).
    /// </summary>
    public int Amount { get; set; }
}