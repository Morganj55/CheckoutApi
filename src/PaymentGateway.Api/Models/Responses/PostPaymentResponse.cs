namespace PaymentGateway.Api.Models.Responses;

/// <summary>
/// Represents the response model returned to the client after a **new payment has been processed** (HTTP POST).
/// This model contains the key details of the resulting transaction.
/// </summary>
public class PostPaymentResponse
{
    /// <summary>
    /// Gets or sets the unique identifier (GUID) of the newly created payment transaction.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the status of the payment attempt (e.g., Authorized, Failed).
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the **last four digits** of the card number used for the transaction.
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
    /// Gets or sets the ISO 4217 **currency code** of the transaction (e.g., "GBP", "EUR").
    /// </summary>
    public string Currency { get; set; }

    /// <summary>
    /// Gets or sets the **monetary amount** of the payment, typically in the smallest currency unit (e.g., cents or pence).
    /// </summary>
    public int Amount { get; set; }
}
