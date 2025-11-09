using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Domain
{
    /// <summary>
    /// Represents the raw, standard response received from the Acquiring Bank.
    /// This is what the AcquiringBankClient returns to the PaymentService.
    /// </summary>
    public class PostBankResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the payment or transaction was successfully **authorized** by the bank or payment processor.
        /// </summary>
        public bool Authorized { get; set; }

        /// <summary>
        /// Gets or sets the **authorization code** (or approval code) returned by the bank or payment processor for a successful transaction.
        /// This code serves as proof that the transaction was approved.
        /// </summary>
        public string AuthorizationCode { get; set; }
    }
}
