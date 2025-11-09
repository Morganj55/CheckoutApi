using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Domain
{
    /// <summary>
    /// Represents the complete details of a processed payment request suitable for internal storage or external retrieval.
    /// </summary>
    public class PaymentRequestResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier (GUID) assigned to the payment transaction.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the current status of the payment (e.g., Authorized, Failed, Success).
        /// </summary>
        public PaymentStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the last four digits of the card number used for the payment.
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
        /// Gets or sets the ISO 4217 currency code of the transaction (e.g., "GBP", "USD").
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the monetary amount of the payment in the smallest unit of the currency (e.g., cents for USD, pence for GBP).
        /// </summary>
        public int Amount { get; set; }
    }
}
