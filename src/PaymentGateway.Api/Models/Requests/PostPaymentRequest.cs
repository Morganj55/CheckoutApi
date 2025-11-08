using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Models.Requests;

/// <summary>
/// Represents a request to process a payment, including card details, amount, and currency.
/// </summary>
/// <remarks>This class is used to encapsulate the necessary information for processing a payment transaction. It
/// includes validation attributes to ensure that the provided data adheres to the expected format and constraints. The
/// card expiry date is validated to ensure it is in the future.</remarks>
public class PostPaymentRequest : IValidatableObject
{
    #region Properties

    [Required(ErrorMessage = "Card number is required.")]
    [MinLength(14, ErrorMessage = "Card number must be at least 14 digits.")]
    [MaxLength(19, ErrorMessage = "Card number must be at most 19 digits.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Card number must contain digits only.")]
    public string CardNumber { get; set; }

    [Required(ErrorMessage = "Expiry month is required.")]
    [Range(1,12, ErrorMessage = "Expiry month must be between 1 and 12.")]
    public int ExpiryMonth { get; set; }

    [Required(ErrorMessage = "Expiry year is required.")]
    public int ExpiryYear { get; set; }

    [Required(ErrorMessage = "Currency code is required.")]
    [StringLength(3, ErrorMessage = "Currency code must be 3 characters long.")]
    [ValidCurrency(ErrorMessage = "Invalid currency code.")]
    public string Currency { get; set; }

    [Required(ErrorMessage = "Payment amount is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "The amount must be a positive number (at least 1 minor unit).")]
    public int Amount { get; set; }

    [Required(ErrorMessage = "Cvv is required.")]
    [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be 3 or 4 digits.")]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must only contain numeric characters.")]
    public string Cvv { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Validates the expiry date of a card based on the provided validation context.
    /// </summary>
    /// <remarks>A card is considered expired if the current date is on or after the first day of the month
    /// following the expiry month. The validation checks both the expiry month and year to determine whether the card
    /// is valid.</remarks>
    /// <param name="validationContext">The context in which the validation is performed. This typically includes information about the object being
    /// validated.</param>
    /// <returns>An <see cref="IEnumerable{ValidationResult}"/> containing validation errors, if any.  If the expiry date is in
    /// the past, a <see cref="ValidationResult"/> is returned indicating the error;  otherwise, the sequence is empty.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Get the current date (we only care about month and year).
        var today = DateTime.UtcNow;

        // A card expires at the end of its expiry month.
        // So, we construct the first day of the next month.
        var firstDayOfNextMonth = new DateTime(ExpiryYear, ExpiryMonth, 1).AddMonths(1);

        // If today's date is on or after the first day of the next month,
        // then the card has expired.
        // Example: 
        // Card: 11/2025. FirstDayOfNextMonth = 12/01/2025.
        // Today: 11/30/2025. 11/30 < 12/01. -> VALID.
        // Today: 12/01/2025. 12/01 >= 12/01. -> EXPIRED.
        if (today.Date >= firstDayOfNextMonth.Date)
        {
            // The validation has failed.
            yield return new ValidationResult(
                "The expiry date must be in the future.",
                new[] { nameof(ExpiryMonth), nameof(ExpiryYear) }
            );
        }
    }

    #endregion
}