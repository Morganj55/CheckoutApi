using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Models.Requests;

/// <summary>
/// Represents a request to process a payment, including card details, amount, and currency.
/// </summary>
/// <remarks>This class is used to encapsulate the necessary information for processing a payment transaction. It
/// includes validation attributes to ensure that the provided data adheres to the expected format and constraints. The
/// card expiry date is validated to ensure it is in the future.</remarks>
public class PostPaymentRequest
{
    #region Properties

    [Required(ErrorMessage = "Card number is required.")]
    public string CardNumber { get; set; }

    [Required(ErrorMessage = "Expiry month is required.")]
    [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12.")]
    public int ExpiryMonth { get; set; }

    [Required(ErrorMessage = "Expiry year is required.")]
    public int ExpiryYear { get; set; }

    [Required(ErrorMessage = "Currency code is required.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters long.")]
    public string Currency { get; set; }

    [Required(ErrorMessage = "Payment amount is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "The amount must be a positive number (at least 1 minor unit).")]
    public int Amount { get; set; }

    [Required(ErrorMessage = "Cvv is required.")]
    [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be 3 or 4 characters.")]
    public string Cvv { get; set; }

    #endregion
}