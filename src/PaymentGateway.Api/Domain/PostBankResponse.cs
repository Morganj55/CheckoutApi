using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Domain
{
    /// <summary>
    /// Represents the raw, standard response received from the Acquiring Bank.
    /// This is what the AcquiringBankClient returns to the PaymentService.
    /// </summary>
    public class PostBankResponse
    {
        [JsonPropertyName("authorized")]
        public bool Authorized { get; set; }

        [JsonPropertyName("authorization_code")]
        public string AuthorizationCode { get; set; }
    }
}
