using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Utility;

using System.Collections.Concurrent;

namespace PaymentGateway.Api.Services;

/// <summary>
/// Provides methods for managing and retrieving payment records.
/// </summary>
/// <remarks>This repository maintains an in-memory collection of payments and allows adding new payments  and
/// retrieving existing ones by their unique identifier. It is intended for use in scenarios  where a simple, in-memory
/// data store is sufficient.</remarks>
public class PaymentsRepository : IPaymentRepository
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentsRepository"/> class.
    /// </summary>
    public PaymentsRepository()
    {
        Payments = new();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the total number of payments in the collection.
    /// </summary>
    public int TotalPaymentCount => Payments.Count;

    /// <summary>
    /// Gets the collection of payment responses.
    /// </summary>
    private ConcurrentDictionary<Guid, PaymentRequestResponse> Payments { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a new payment response object to the collection of payments.
    /// </summary>
    /// <param name="payment">The <see cref="PostPaymentResponse"/> object to be added.</param>
    public OperationResult<bool> Add(PaymentRequestResponse payment)
    {
        if (Payments.TryAdd(payment.Id, payment))
        {
            return OperationResult<bool>.Success(true);
        }

        return OperationResult<bool>.Failure(ErrorKind.Unexpected, "Could not add payment", null);
    }

    /// <summary>
    /// Retrieves a specific payment response from the collection based on its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the payment response to retrieve.</param>
    /// <returns>
    /// The matching <see cref="PostPaymentResponse"/> object if found; otherwise, returns <see langword="null"/>.
    /// </returns>
    public async Task<OperationResult<PaymentRequestResponse>> GetAsync(Guid id)
    {
        if (Payments.TryGetValue(id, out PaymentRequestResponse value))
        {
            return OperationResult<PaymentRequestResponse>.Success(value);
        }

        return OperationResult<PaymentRequestResponse>.Failure(ErrorKind.NotFound, "Payment not found.", System.Net.HttpStatusCode.NotFound);
    }

    #endregion
}