using System.ComponentModel.DataAnnotations;
using System.Transactions;

using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Validation
{
    /// <summary>
    /// Specifies that a data field value must be a valid ISO 4217 currency code.
    /// </summary>
    /// <remarks>This attribute validates that the value of the associated property or field is either null,
    /// empty,  or a valid ISO 4217 currency code. If the value is null or empty, the validation passes. Otherwise,  the
    /// value is checked against a predefined list of valid ISO currency codes.</remarks>
    public class ValidCurrencyAttribute : ValidationAttribute
    {
        /// <summary>
        /// Validates whether the specified value is a supported ISO currency code.
        /// </summary>
        /// <remarks>This method checks the provided value against a predefined set of supported ISO
        /// currency codes.  If the value is null or empty, the validation is considered successful. If the value is not
        /// a valid  ISO currency code, the method returns a validation error with a customizable error
        /// message.</remarks>
        /// <param name="value">The value to validate, expected to be a string representing a currency code.</param>
        /// <param name="validationContext">The context in which the validation is performed. This parameter is not used in the validation logic.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating whether the validation was successful.  Returns <see
        /// cref="ValidationResult.Success"/> if the value is null, empty, or a valid ISO currency code;  otherwise,
        /// returns a <see cref="ValidationResult"/> with an error message.</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string currencyCode = value as string;
            if (string.IsNullOrEmpty(currencyCode))
            {
                return ValidationResult.Success;
            }

            // Access the static HashSet directly
            if (IsoCurrencyCodes.Codes.Contains(currencyCode))
            {
                return ValidationResult.Success; 
            }

            return new ValidationResult(ErrorMessage ?? $"The currency code '{currencyCode}' is not supported.");
        }
    }
}
