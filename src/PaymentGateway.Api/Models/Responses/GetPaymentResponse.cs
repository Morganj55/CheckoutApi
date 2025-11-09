using System.Collections.Generic;

namespace PaymentGateway.Api.Models.Responses;

/// <summary>
/// Represents the response model returned to the client when retrieving the details of a specific, 
/// previously processed payment.
/// </summary>
public class GetPaymentResponse
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPaymentResponse"/> class.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the processed payment transaction.</param>
    /// <param name="status">The final or current status of the payment (e.g., Authorized, Failed).</param>
    /// <param name="cardNumberLastFour">The last four digits of the card number used for the transaction.</param>
    /// <param name="expiryMonth">The expiry month of the card.</param>
    /// <param name="expiryYear">The expiry year of the card.</param>
    /// <param name="currency">The ISO 4217 currency code of the transaction (e.g., "GBP").</param>
    /// <param name="amount">The monetary amount of the payment, typically in the smallest currency unit.</param>
    public GetPaymentResponse(
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

        CardNumberLastFour = cardNumberLastFour;
        Id = id;
        Status = status;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Currency = currency;
        Amount = amount;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the unique identifier (GUID) of the processed payment transaction.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the final or current status of the payment (e.g., Authorized, Failed).
    /// </summary>
    public PaymentStatus Status { get; }

    /// <summary>
    /// Gets the last four digits of the card number used for the transaction.
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
    /// Gets the ISO 4217 currency code of the transaction (e.g., "GBP").
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Gets the monetary amount of the payment, typically in the smallest currency unit (e.g., cents or pence).
    /// </summary>
    public int Amount { get; }

    #endregion
}