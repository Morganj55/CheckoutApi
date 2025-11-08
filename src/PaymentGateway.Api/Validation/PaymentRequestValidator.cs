using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Validation
{
    public static class PaymentRequestValidator
    {
        private static readonly Regex DigitsOnly = new(@"^\d+$", RegexOptions.Compiled);
        private static readonly Regex CvvDigits = new(@"^\d{3,4}$", RegexOptions.Compiled);

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
            AddIfNotNull(errors, ValidateExpiryDateInFuture(expiryMonth, expiryYear));

            return errors;
        }

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

        public static ValidationResult? ValidateExpiryMonth(int month)
        {
            const string member = nameof(PostPaymentRequest.ExpiryMonth);

            if (month == default)
                return Failure("Expiry month is required.", member);

            if (month < 1 || month > 12)
                return Failure("Expiry month must be between 1 and 12.", member);

            return null;
        }

        public static ValidationResult? ValidateExpiryYear(int year)
        {
            const string member = nameof(PostPaymentRequest.ExpiryYear);

            if (year == default)
                return Failure("Expiry year is required.", member);

            if (year < 1)
                return Failure("Expiry year is invalid.", member);

            return null;
        }

        public static ValidationResult? ValidateCurrency(string currency)
        {
            const string member = nameof(PostPaymentRequest.Currency);

            if (string.IsNullOrWhiteSpace(currency))
                return Failure("Currency code is required.", member);

            if (currency.Length != 3)
                return Failure("Currency code must be 3 characters long.", member);

            if (!IsoCurrencyCodes.Codes.Contains(currency))
                return Failure("Invalid currency code.", member);

            return null;
        }

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
        /// </summary>
        public static ValidationResult? ValidateExpiryDateInFuture(int expiryMonth, int expiryYear)
        {
            // Only run if month/year are individually valid.
            if (ValidateExpiryMonth(expiryMonth) is not null) return null;
            if (ValidateExpiryYear(expiryYear) is not null) return null;

            try
            {
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
                return Failure(
                    "Expiry month/year is invalid.",
                    nameof(PostPaymentRequest.ExpiryMonth),
                    nameof(PostPaymentRequest.ExpiryYear));
            }

            return null;
        }

        private static void AddIfNotNull(List<ValidationResult> list, ValidationResult? maybe)
        {
            if (maybe is not null) list.Add(maybe);
        }

        private static ValidationResult Failure(string message, params string[] members) =>
            members is { Length: > 0 }
                ? new ValidationResult(message, members)
                : new ValidationResult(message);
    }
}
