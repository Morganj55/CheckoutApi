using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Validation
{
    /// <summary>
    /// Provides static utility methods for validating the individual fields of a <see cref="PostPaymentRequest"/>.
    /// </summary>
    public static class PaymentRequestValidator
    {
        #region Fields

        /// <summary>
        /// Regular expression to ensure a string contains only digits. Compiled for performance.
        /// </summary>
        private static readonly Regex DigitsOnly = new(@"^\d+$", RegexOptions.Compiled);

        /// <summary>
        /// Regular expression to ensure a string is 3 or 4 digits long (standard CVV/CVC length). Compiled for performance.
        /// </summary>
        private static readonly Regex CvvDigits = new(@"^\d{3,4}$", RegexOptions.Compiled);

        #endregion

        #region Public Validation Methods

        /// <summary>
        /// Runs all individual validation checks on the payment request data.
        /// </summary>
        /// <param name="cardNumber">The card number string.</param>
        /// <param name="expiryMonth">The card expiry month (1-12).</param>
        /// <param name="expiryYear">The card expiry year.</param>
        /// <param name="currency">The 3-character ISO currency code.</param>
        /// <param name="amount">The payment amount in the minor currency unit.</param>
        /// <param name="cvv">The CVV/CVC code string.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ValidationResult"/> objects detailing all failures; an empty list if validation succeeds.</returns>
        public static List<ValidationResult> Validate(string cardNumber,
            int expiryMonth,
            int expiryYear,
            string currency,
            int amount,
            string cvv)
        {
            var errors = new List<ValidationResult>();

            AddIfNotNull(errors, ValidateCardNumber(cardNumber));
            AddIfNotNull(errors, ValidateExpiryMonth(expiryMonth));
            AddIfNotNull(errors, ValidateExpiryYear(expiryYear));
            AddIfNotNull(errors, ValidateCurrency(currency));
            AddIfNotNull(errors, ValidateAmount(amount));
            AddIfNotNull(errors, ValidateCvv(cvv));

            // Validate the composite date (only if individual month/year are valid)
            AddIfNotNull(errors, ValidateExpiryDateInFuture(expiryMonth, expiryYear));

            return errors;
        }

        /// <summary>
        /// Validates the card number format and length.
        /// </summary>
        /// <param name="cardNumber">The card number string to validate.</param>
        /// <returns>A <see cref="ValidationResult"/> on failure, or <c>null</c> on success.</returns>
        public static ValidationResult? ValidateCardNumber(string cardNumber)
        {
            const string member = nameof(PostPaymentRequest.CardNumber);

            if (string.IsNullOrWhiteSpace(cardNumber))
                return Failure("Card number is required.", member);

            if (cardNumber.Length < 14)
                return Failure("Card number must be at least 14 digits.", member);

            if (cardNumber.Length > 19)
                return Failure("Card number must be at most 19 digits.", member);

            if (!DigitsOnly.IsMatch(cardNumber))
                return Failure("Card number must contain digits only.", member);

            return null;
        }

        /// <summary>
        /// Validates the card expiry month (must be between 1 and 12).
        /// </summary>
        /// <param name="month">The expiry month integer.</param>
        /// <returns>A <see cref="ValidationResult"/> on failure, or <c>null</c> on success.</returns>
        public static ValidationResult? ValidateExpiryMonth(int month)
        {
            const string member = nameof(PostPaymentRequest.ExpiryMonth);

            if (month == default)
                return Failure("Expiry month is required.", member);

            if (month < 1 || month > 12)
                return Failure("Expiry month must be between 1 and 12.", member);

            return null;
        }

        /// <summary>
        /// Validates the card expiry year (must be present and a positive number).
        /// </summary>
        /// <param name="year">The expiry year integer.</param>
        /// <returns>A <see cref="ValidationResult"/> on failure, or <c>null</c> on success.</returns>
        public static ValidationResult? ValidateExpiryYear(int year)
        {
            const string member = nameof(PostPaymentRequest.ExpiryYear);

            if (year == default)
                return Failure("Expiry year is required.", member);

            if (year < 1)
                return Failure("Expiry year is invalid.", member);

            return null;
        }

        /// <summary>
        /// Validates the currency code (must be 3 characters long and be a recognised ISO currency code).
        /// </summary>
        /// <param name="currency">The currency code string.</param>
        /// <returns>A <see cref="ValidationResult"/> on failure, or <c>null</c> on success.</returns>
        public static ValidationResult? ValidateCurrency(string currency)
        {
            const string member = nameof(PostPaymentRequest.Currency);

            if (string.IsNullOrWhiteSpace(currency))
                return Failure("Currency code is required.", member);

            if (currency.Length != 3)
                return Failure("Currency code must be 3 characters long.", member);

            // Assuming IsoCurrencyCodes.Codes is a collection of valid ISO 4217 codes
            if (!IsoCurrencyCodes.Codes.Contains(currency))
                return Failure("Invalid currency code.", member);

            return null;
        }

        /// <summary>
        /// Validates the payment amount (must be present and a positive number).
        /// </summary>
        /// <param name="amount">The payment amount integer.</param>
        /// <returns>A <see cref="ValidationResult"/> on failure, or <c>null</c> on success.</returns>
        public static ValidationResult? ValidateAmount(int amount)
        {
            const string member = nameof(PostPaymentRequest.Amount);

            if (amount == default)
                return Failure("Payment amount is required.", member);

            if (amount < 1)
                return Failure("The amount must be a positive number (at least 1 minor unit).", member);

            if (amount > int.MaxValue)
                return Failure($"The amount exceeds the maximum allowed value of {int.MaxValue}.", member);

            return null;
        }

        /// <summary>
        /// Validates the CVV/CVC code (must be 3 or 4 digits and contain only numeric characters).
        /// </summary>
        /// <param name="cvv">The CVV/CVC code string.</param>
        /// <returns>A <see cref="ValidationResult"/> on failure, or <c>null</c> on success.</returns>
        public static ValidationResult? ValidateCvv(string cvv)
        {
            const string member = nameof(PostPaymentRequest.Cvv);

            if (string.IsNullOrWhiteSpace(cvv))
                return Failure("Cvv is required.", member);

            if (cvv.Length < 3 || cvv.Length > 4)
                return Failure("CVV must be 3 or 4 digits.", member);

            if (!CvvDigits.IsMatch(cvv))
                return Failure("CVV must only contain numeric characters.", member);

            return null;
        }

        /// <summary>
        /// A card expires at the end of its expiry month.
        /// It’s invalid on/after the first day of the next month (UTC).
        /// This check should only run if <see cref="ValidateExpiryMonth"/> and <see cref="ValidateExpiryYear"/> have already passed.
        /// </summary>
        /// <param name="expiryMonth">The card expiry month.</param>
        /// <param name="expiryYear">The card expiry year.</param>
        /// <returns>A <see cref="ValidationResult"/> on failure, or <c>null</c> on success.</returns>
        public static ValidationResult? ValidateExpiryDateInFuture(int expiryMonth, int expiryYear)
        {
            // Only run if month/year are individually valid.
            if (ValidateExpiryMonth(expiryMonth) is not null) return null;
            if (ValidateExpiryYear(expiryYear) is not null) return null;

            try
            {
                // Calculate the first day of the month *after* the expiry month (e.g., if expiry is 03/2026, check 04/01/2026).
                var firstDayOfNextMonth = new DateTime(expiryYear, expiryMonth, 1, 0, 0, 0, DateTimeKind.Utc)
                                                .AddMonths(1);

                if (DateTime.UtcNow.Date >= firstDayOfNextMonth.Date)
                {
                    return Failure(
                        "The expiry date must be in the future.",
                        nameof(PostPaymentRequest.ExpiryMonth),
                        nameof(PostPaymentRequest.ExpiryYear));
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Should be caught by individual month/year validation, but included as a safeguard.
                return Failure(
                    "Expiry month/year is invalid.",
                    nameof(PostPaymentRequest.ExpiryMonth),
                    nameof(PostPaymentRequest.ExpiryYear));
            }

            return null;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Utility method to add a non-null <see cref="ValidationResult"/> to a list of errors.
        /// </summary>
        /// <param name="list">The list to add the result to.</param>
        /// <param name="maybe">The nullable <see cref="ValidationResult"/>.</param>
        private static void AddIfNotNull(List<ValidationResult> list, ValidationResult? maybe)
        {
            if (maybe is not null) list.Add(maybe);
        }

        /// <summary>
        /// Factory method to create a <see cref="ValidationResult"/> for a failure, optionally associating it with specific member names.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="members">The names of the members that failed validation.</param>
        /// <returns>A new <see cref="ValidationResult"/> instance.</returns>
        private static ValidationResult Failure(string message, params string[] members) =>
            members is { Length: > 0 }
                ? new ValidationResult(message, members)
                : new ValidationResult(message);

        #endregion
    }
}
