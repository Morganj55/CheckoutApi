using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Clients
{
    /// <summary>
    /// Represents a client for interacting with the ExampleBank acquiring bank API to process payment requests.
    /// </summary>
    /// <remarks>This class provides functionality to send payment requests to the ExampleBank acquiring bank
    /// and retrieve the corresponding responses. It implements the <see cref="IAquiringBankClient"/> interface,
    /// ensuring compatibility with systems that rely on this abstraction.</remarks>
    public class ExampleBank : IAquiringBankClient
    {
        private readonly HttpClient _httpClient;

        public ExampleBank(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PostBankResponse> ProcessPaymentAsync(PaymentRequestCommand request)
        {
           throw new NotImplementedException();
        }

    }
}
