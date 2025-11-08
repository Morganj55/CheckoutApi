using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Clients
{
    public interface IAquiringBankClient
    {
        /// <summary>
        /// Processes a payment request asynchronously and returns the response from the payment gateway.
        /// </summary>
        /// <param name="request">The payment request containing the details required to process the payment.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response from the payment
        /// gateway, including the status and any additional information about the processed payment.</returns>
        Task<OperationResult<PostBankResponse>> ProcessPaymentAsync(PaymentRequestCommand request);
    }
}
