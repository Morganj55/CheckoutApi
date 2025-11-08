using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

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
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the collection of payment responses.
    /// </summary>
    public List<PostPaymentResponse> Payments = new();

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a new payment response object to the collection of payments.
    /// </summary>
    /// <param name="payment">The <see cref="PostPaymentResponse"/> object to be added.</param>
    public void Add(PostPaymentResponse payment)
    {
        Payments.Add(payment);
    }

    /// <summary>
    /// Retrieves a specific payment response from the collection based on its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the payment response to retrieve.</param>
    /// <returns>
    /// The matching <see cref="PostPaymentResponse"/> object if found; otherwise, returns <see langword="null"/>.
    /// </returns>
    public PostPaymentResponse Get(Guid id)
    {
        return Payments.FirstOrDefault(p => p.Id == id);
    }

    #endregion
}