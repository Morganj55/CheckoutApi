using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

using PaymentGateway.Api.Validation;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PaymentGateway.Api.Domain
{
    public class PaymentRequestCommand
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentRequestCommand"/> class.
        /// </summary>
        /// <param name="cardNumber">The primary account number.</param>
        /// <param name="expiryMonth">The card expiry month (1–12).</param>
        /// <param name="expiryYear">The four-digit card expiry year.</param>
        /// <param name="currency">The ISO 4217 currency code.</param>
        /// <param name="amount">The payment amount in minor units.</param>
        /// <param name="cvv">The card verification value.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="cardNumber"/>, <paramref name="currency"/>, or <paramref name="cvv"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// This constructor assumes inputs have already been validated by the static factory method.
        /// It performs minimal defensive checks and normalization (currency trimming and upper-casing).
        /// </remarks>
        private PaymentRequestCommand(string cardNumber, int expiryMonth, int expiryYear, string currency, int amount, string cvv)
        {
            CardNumber = cardNumber ?? throw new ArgumentNullException(nameof(cardNumber));
            Currency = (currency ?? throw new ArgumentNullException(nameof(currency))).Trim().ToUpperInvariant();
            Amount = amount; // Assume validated by factory (e.g., >= 1)
            Cvv = cvv ?? throw new ArgumentNullException(nameof(cvv));
            ExpiryMonth = expiryMonth;
            ExpiryYear = expiryYear;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the card primary account number (PAN).
        /// </summary>
        [JsonPropertyName("card_number")]
        public string CardNumber { get; private set; }

        /// <summary>
        /// Gets the card expiry month (1–12).
        /// </summary>
        public int ExpiryMonth { get; private set; }

        /// <summary>
        /// Gets the card expiry year (four digits).
        /// </summary>
        public int ExpiryYear { get; private set; }

        /// <summary>
        /// Gets the formatted expiry date as <c>MM/YYYY</c>.
        /// </summary>
        [JsonPropertyName("expiry_date")]
        public string ExpiryDate => FormatExpiryDate(ExpiryMonth, ExpiryYear);

        /// <summary>
        /// Gets the ISO 4217 currency code (upper-case).
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; private set; }

        /// <summary>
        /// Gets the payment amount in minor units (e.g., cents).
        /// </summary>
        [JsonPropertyName("amount")]
        public int Amount { get; private set; }

        /// <summary>
        /// Gets the card verification value (CVV).
        /// </summary>
        [JsonPropertyName("cvv")]
        public string Cvv { get; private set; }

        #endregion

        #region Static Methods

        /// <summary>
        /// Attempts to create a <see cref="PaymentRequestCommand"/> from raw values.
        /// </summary>
        /// <param name="cardNumber">The card number.</param>
        /// <param name="expiryMonth">The expiry month (1–12).</param>
        /// <param name="expiryYear">The expiry year (four digits).</param>
        /// <param name="currency">The ISO 4217 currency code.</param>
        /// <param name="amount">The amount in minor units.</param>
        /// <param name="cvv">The card verification value.</param>
        /// <param name="command">When successful, the constructed command.</param>
        /// <param name="errors">If validation fails, the list of validation errors.</param>
        /// <returns>
        /// <c>true</c> if the values are valid and <paramref name="command"/> was created; otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Validation is performed by <see cref="PaymentRequestValidator"/>. Currency is trimmed and upper-cased
        /// during construction; <see cref="ExpiryDate"/> is derived from <paramref name="expiryMonth"/> and
        /// <paramref name="expiryYear"/>.
        /// </remarks>
        public static bool TryCreate(
            string cardNumber,
            int expiryMonth,
            int expiryYear,
            string currency,
            int amount,
            string cvv,
            out PaymentRequestCommand command,
            out List<ValidationResult> errors)
        {
            errors = PaymentRequestValidator.Validate(cardNumber, expiryMonth, expiryYear, currency, amount, cvv);
            if (errors.Count == 0)
            {
                command = new PaymentRequestCommand(cardNumber, expiryMonth, expiryYear, currency, amount, cvv);
                return true;
            }

            command = null!;
            return false;
        }

        /// <summary>
        /// Formats an expiry month and year as <c>MM/YYYY</c>.
        /// </summary>
        /// <param name="month">The month (1–12).</param>
        /// <param name="year">The year (four digits preferred).</param>
        /// <returns>A string formatted as <c>MM/YYYY</c>.</returns>
        public static string FormatExpiryDate(int month, int year)
        {
            return $"{month:D2}/{year:D4}";
        }

        private static readonly JsonSerializerOptions DefaultJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Serializes this command to JSON with snake_case keys expected by the bank.
        /// </summary>
        /// <param name="options">
        /// Optional <see cref="JsonSerializerOptions"/>. When <c>null</c>, sensible defaults are used
        /// (ignore nulls, case-insensitive read, no naming policy since keys are explicit).
        /// </param>
        /// <returns>A JSON string representing this command.</returns>
        public string ToJson(JsonSerializerOptions? options = null)
        {
            var payload = new
            {
                card_number = CardNumber,
                expiry_date = ExpiryDate,   // "MM/YYYY"
                currency = Currency,     // e.g., "USD"
                amount = Amount,       // minor units
                cvv = Cvv
            };

            return JsonSerializer.Serialize(payload, options ?? DefaultJsonOptions);
        }

        #endregion
    }
}
