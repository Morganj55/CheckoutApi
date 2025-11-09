using System.Net;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Utility;
using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Services
{
    /// <summary>
    /// Provides functionality for processing payments, including creating and storing payment records.
    /// </summary>
    /// <remarks>This service is responsible for handling payment processing requests and generating responses
    /// that include payment details. It interacts with an underlying payment repository to persist  payment data. The
    /// service is designed to work asynchronously and supports operations such as  processing payment
    /// requests.</remarks>
    public class PaymentService : IPaymentService
    {
        #region Fields

        /// <summary>
        /// The repository used for accessing and persisting payment data.
        /// </summary>
        private readonly IPaymentRepository _paymentRepository;

        /// <summary>
        /// The aquiring bank client
        /// </summary>
        private readonly IAcquiringBankClient _bankClient;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentService"/> class.
        /// </summary>
        /// <param name="paymentRepository">The payment repository instance used for data access, injected via dependency injection.</param>
        public PaymentService(IPaymentRepository paymentRepository, IAcquiringBankClient bankClient)
        {
            _paymentRepository = paymentRepository;
            _bankClient = bankClient;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Processes a payment request asynchronously, creates a response, and persists the transaction.
        /// </summary>
        /// <param name="request">The <see cref="PostPaymentRequest"/> containing the payment details.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, which upon completion
        /// returns the generated <see cref="PostPaymentResponse"/> object.
        /// </returns>
        public async Task<OperationResult<PaymentRequestResponse>> ProcessPaymentAsync(PaymentRequestCommand request)
        {
            var paymentRecord = new PaymentRequestResponse(
                Guid.NewGuid(),
                PaymentStatus.Pending,
                request.CardNumber,
                request.ExpiryMonth,
                request.ExpiryYear,
                request.Currency,
                request.Amount);

            // Persist the payment record before processing
            var addResult = await _paymentRepository.Add(paymentRecord);
            if (addResult.IsFailure)
            {
                return OperationResult<PaymentRequestResponse>.Failure(ErrorKind.Unexpected, "Could not add payment", null);
            }

            try
            {
                // Process the payment with the acquiring bank
                var bankResult = await _bankClient.ProcessPaymentAsync(request);
                if (bankResult.IsFailure)
                {
                    return OperationResult<PaymentRequestResponse>.Failure(bankResult.Error!);
                }

                // Update the payment record status based on the bank's response
                var newPaymentStatus = bankResult.Data!.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined;
                var updatePaymentResult = await _paymentRepository.UpdatePaymentStatus(paymentRecord.Id, newPaymentStatus);
                if (updatePaymentResult.IsFailure)
                {
                    // Need a way to reconcile later, could do through logging of payment ID as it should exist within storage already?
                    return OperationResult<PaymentRequestResponse>.Failure(ErrorKind.Transient, "Payment authorized but recording failed. We will reconcile.", HttpStatusCode.Accepted);
                }

                return OperationResult<PaymentRequestResponse>.Success(updatePaymentResult.Data!);
            }
            catch
            {
                var updateErrorRes = await _paymentRepository.UpdatePaymentStatus(paymentRecord.Id, PaymentStatus.InternalError);
                if (updateErrorRes.IsFailure)
                {
                    // Need a way to reconcile later, could do through logging of payment ID as it should exist within storage already?
                }

                throw;
            }
        }

        /// <summary>
        /// Retrieves the payment details for the specified payment identifier.
        /// </summary>
        /// <remarks>This method retrieves payment information from the underlying data store. Ensure that
        /// the provided <paramref name="id"/> corresponds to an existing payment record.</remarks>
        /// <param name="id">The unique identifier of the payment to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see
        /// cref="OperationResult{T}"/> object that includes the payment details if the operation is successful, or an
        /// error message if it fails.</returns>
        public async Task<OperationResult<PaymentRequestResponse>> GetPaymentAsync(Guid id)
        {
            return await _paymentRepository.GetAsync(id);
        }

        #endregion
    }
}