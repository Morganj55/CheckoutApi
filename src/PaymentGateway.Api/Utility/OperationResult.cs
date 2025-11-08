using System.Net;

using PaymentGateway.Api.Domain;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PaymentGateway.Api.Utility
{
    public enum ErrorKind { Validation, Conflict, NotFound, Transient, Unexpected }

    public sealed record Error(ErrorKind Kind, string Message, HttpStatusCode Code);

    public class OperationResult<T>
    {
        private OperationResult(T? data, bool isSuccess, Error? error)
        {
            IsSuccess = isSuccess;
            Error = error;
            Data = data;
        }

        public bool IsSuccess { get; }
        public Error? Error { get; }
        public bool IsFailure => !IsSuccess;

        public T? Data { get; }

        public static OperationResult<T> Success(T data) => new(data, true, null);

        public static OperationResult<T> Failure(ErrorKind errorType, string message, HttpStatusCode code)=> new(default, false, new Error(errorType , message, code));

        public static OperationResult<T> Failure(ErrorKind errorType, Exception ex, HttpStatusCode code) => Failure(errorType,  ex.Message, code);

        public static OperationResult<T> Failure(Error error) => Failure(error.Kind, error.Message, error.Code);

    }
}
