using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Domain;
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
        private readonly IAquiringBankClient _bankClient;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentService"/> class.
        /// </summary>
        /// <param name="paymentRepository">The payment repository instance used for data access, injected via dependency injection.</param>
        public PaymentService(IPaymentRepository paymentRepository, IAquiringBankClient bankClient)
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
        public async Task<PaymentRequestResponse> ProcessPaymentAsync(PaymentRequestCommand request)
        {
            var res = await _bankClient.ProcessPaymentAsync(request);
            var paymentResponse = new PaymentRequestResponse
            {
                Id = Guid.NewGuid(),
                Amount = request.Amount,
                Currency = request.Currency,
                CardNumberLastFour = request.CardNumber.Substring(request.CardNumber.Length - 4, 4),
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Status = res.Authorized ? Models.PaymentStatus.Authorized : Models.PaymentStatus.Declined,
            };

            if (res.Authorized)
            {
                // Persist the payment record locally
                _paymentRepository.Add(paymentResponse);
            }

            return paymentResponse;
        }

        #endregion
    }
}