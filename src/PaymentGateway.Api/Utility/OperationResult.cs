using System.Net;

using PaymentGateway.Api.Domain;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PaymentGateway.Api.Utility
{
    /// <summary>
    /// Defines the broad categories of errors that can occur during an operation.
    /// </summary>
    public enum ErrorKind
    {
        /// <summary>The operation failed due to invalid input data or business rule violation.</summary>
        Validation,

        /// <summary>The operation failed because the requested action conflicts with the current state (e.g., resource already exists).</summary>
        Conflict,

        /// <summary>The requested resource could not be found.</summary>
        NotFound,

        /// <summary>A temporary, retryable error occurred, usually due to external service unavailability or timeout.</summary>
        Transient,

        /// <summary>An unexpected or uncategorized internal error occurred.</summary>
        Unexpected
    }

    /// <summary>
    /// Represents a detailed error structure, containing the type of error, a descriptive message, and an optional HTTP status code.
    /// </summary>
    /// <param name="Kind">The type of error, categorized by <see cref="ErrorKind"/>.</param>
    /// <param name="Message">A descriptive message detailing the specific reason for the failure.</param>
    /// <param name="Code">An optional HTTP status code associated with the error (e.g., <see cref="HttpStatusCode.BadRequest"/>).</param>
    public sealed record Error(ErrorKind Kind, string Message, HttpStatusCode? Code);

    /// <summary>
    /// Represents the result of an operation that can either succeed and return data of type <typeparamref name="T"/>, 
    /// or fail and return an <see cref="Error"/> object. This is an implementation of the Railway-Oriented Programming pattern.
    /// </summary>
    /// <typeparam name="T">The type of data expected on a successful operation.</typeparam>
    public class OperationResult<T>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationResult{T}"/> class.
        /// </summary>
        /// <param name="data">The result data for successful operations.</param>
        /// <param name="isSuccess">A flag indicating whether the operation succeeded.</param>
        /// <param name="error">The error details for failed operations.</param>
        private OperationResult(T? data, bool isSuccess, Error? error)
        {
            IsSuccess = isSuccess;
            Error = error;
            Data = data;
        }

        #endregion

        #region Propeties

        /// <summary>
        /// Gets a value indicating whether the operation succeeded.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the detailed error information if the operation failed; otherwise, null.
        /// </summary>
        public Error? Error { get; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the resulting data if the operation succeeded; otherwise, the default value for <typeparamref name="T"/>.
        /// </summary>
        public T? Data { get; }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Creates a successful operation result containing the specified data.
        /// </summary>
        /// <param name="data">The data returned by the successful operation.</param>
        /// <returns>A new <see cref="OperationResult{T}"/> instance representing success.</returns>
        public static OperationResult<T> Success(T data) => new(data, true, null);

        /// <summary>
        /// Creates a failed operation result with specified error details.
        /// </summary>
        /// <param name="errorType">The category of the error.</param>
        /// <param name="message">A descriptive message detailing the error.</param>
        /// <param name="code">The optional HTTP status code associated with the error.</param>
        /// <returns>A new <see cref="OperationResult{T}"/> instance representing failure.</returns>
        public static OperationResult<T> Failure(ErrorKind errorType, string message, HttpStatusCode? code)
            => new(default, false, new Error(errorType, message, code));

        /// <summary>
        /// Creates a failed operation result using an <see cref="Exception"/>'s message and a specified status code.
        /// </summary>
        /// <param name="errorType">The category of the error.</param>
        /// <param name="ex">The exception whose message will be used for the failure.</param>
        /// <param name="code">The HTTP status code associated with the error.</param>
        /// <returns>A new <see cref="OperationResult{T}"/> instance representing failure.</returns>
        public static OperationResult<T> Failure(ErrorKind errorType, Exception ex, HttpStatusCode code)
            => Failure(errorType, ex.Message, code);

        /// <summary>
        /// Creates a failed operation result directly from an existing <see cref="Error"/> record.
        /// </summary>
        /// <param name="error">The existing <see cref="Error"/> record.</param>
        /// <returns>A new <see cref="OperationResult{T}"/> instance representing failure.</returns>
        public static OperationResult<T> Failure(Error error)
            => new(default, false, error);

        #endregion
    }
}
