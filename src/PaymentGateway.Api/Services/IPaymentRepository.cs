using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services
{
    /// <summary>
    /// Defines a contract for managing payment records in a data store.
    /// </summary>
    /// <remarks>This interface provides methods to add new payment records and retrieve existing ones by
    /// their unique identifier. Implementations of this interface are responsible for ensuring the persistence and
    /// retrieval of payment data.</remarks>
    public interface IPaymentRepository
    {
        /// <summary>
        /// Adds a payment response to the collection for further processing or storage.
        /// </summary>
        /// <param name="payment">The payment response to add. Cannot be null.</param>
        void Add(PostPaymentResponse payment);

        /// <summary>
        /// Retrieves the payment response associated with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the payment to retrieve.</param>
        /// <returns>A <see cref="PostPaymentResponse"/> object containing the details of the payment. Returns <see
        /// langword="null"/> if no payment is found with the specified identifier.</returns>
        PostPaymentResponse Get(Guid id);
    }
}
