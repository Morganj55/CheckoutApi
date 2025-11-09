using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Services
{
    /// <summary>
    /// Defines a contract for processing payments asynchronously.
    /// </summary>
    /// <remarks>This interface provides a method for handling payment processing operations.  Implementations
    /// of this interface are expected to handle the details of payment processing,  such as interacting with payment
    /// gateways or validating payment requests.</remarks>
    public interface IPaymentService
    {
        /// <summary>
        /// Processes a payment asynchronously based on the provided payment request.
        /// </summary>
        /// <remarks>Ensure that the <paramref name="request"/> object contains all required fields for
        /// successful payment processing. The method performs the operation asynchronously and does not block the
        /// calling thread.</remarks>
        /// <param name="request">The payment request containing the details required to process the payment. Cannot be <see
        /// langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="PostPaymentResponse"/> object with the outcome of the payment processing, including any relevant
        /// status or confirmation details.</returns>
        Task<OperationResult<PaymentRequestResponse>> ProcessPaymentAsync(PaymentRequestCommand request);

        /// <summary>
        /// Retrieves the payment details for the specified payment identifier.
        /// </summary>
        /// <remarks>The method performs an asynchronous operation to fetch payment details. Ensure that
        /// the  provided <paramref name="id"/> corresponds to a valid payment record. If the payment is  not found, the
        /// <see cref="OperationResult{T}.IsSuccess"/> property of the result will be  <see
        /// langword="false"/>.</remarks>
        /// <param name="id">The unique identifier of the payment to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an  <see
        /// cref="OperationResult{T}"/> object with the payment details encapsulated in a  <see
        /// cref="PaymentRequestResponse"/> instance if the operation is successful.</returns>
        Task<OperationResult<PaymentRequestResponse>> GetPaymentAsync(Guid id);
    }
}
