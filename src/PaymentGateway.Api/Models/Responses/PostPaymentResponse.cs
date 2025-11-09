namespace PaymentGateway.Api.Models.Responses;

/// <summary>
/// Represents the response model returned to the client after a **new payment has been processed** (HTTP POST).
/// This model contains the key details of the resulting transaction and is designed to be immutable.
/// </summary>
public class PostPaymentResponse
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PostPaymentResponse"/> class.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the newly created payment transaction.</param>
    /// <param name="status">The status of the payment attempt (e.g., Authorized, Failed).</param>
    /// <param name="cardNumberLastFour">The last four digits of the card number used for the transaction.</param>
    /// <param name="expiryMonth">The expiry month of the card.</param>
    /// <param name="expiryYear">The expiry year of the card.</param>
    /// <param name="currency">The ISO 4217 currency code of the transaction (e.g., "GBP", "EUR").</param>
    /// <param name="amount">The monetary amount of the payment, typically in the smallest currency unit.</param>
    public PostPaymentResponse(
        Guid id,
        PaymentStatus status,
        string cardNumberLastFour,
        int expiryMonth,
        int expiryYear,
        string currency,
        int amount)
    {
        if (string.IsNullOrWhiteSpace(cardNumberLastFour) || cardNumberLastFour.Length != 4 || !cardNumberLastFour.All(char.IsDigit))
            throw new ArgumentException("CardNumberLastFour must be exactly 4 digits.", nameof(cardNumberLastFour));

        Id = id;
        Status = status;
        CardNumberLastFour = cardNumberLastFour;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Currency = currency;
        Amount = amount;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the unique identifier (GUID) of the newly created payment transaction.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the status of the payment attempt (e.g., Authorized, Failed).
    /// </summary>
    public PaymentStatus Status { get; }

    /// <summary>
    /// Gets the **last four digits** of the card number used for the transaction.
    /// </summary>
    public string CardNumberLastFour { get; }

    /// <summary>
    /// Gets the expiry month of the card.
    /// </summary>
    public int ExpiryMonth { get; }

    /// <summary>
    /// Gets the expiry year of the card.
    /// </summary>
    public int ExpiryYear { get; }

    /// <summary>
    /// Gets the ISO 4217 **currency code** of the transaction (e.g., "GBP", "EUR").
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Gets the **monetary amount** of the payment, typically in the smallest currency unit (e.g., cents or pence).
    /// </summary>
    public int Amount { get; }

    #endregion
}
