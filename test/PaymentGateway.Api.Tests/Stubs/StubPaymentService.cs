using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Tests.Stubs
{
    public sealed class StubPaymentService : IPaymentService
    {
        public Func<Guid, CancellationToken, Task<OperationResult<PaymentRequestResponse>>>? GetHandler { get; set; }
        public Func<PaymentRequestCommand, CancellationToken, Task<OperationResult<PaymentRequestResponse>>>? ProcessHandler { get; set; }

        public Guid? LastGetId { get; private set; }

        public Task<OperationResult<PaymentRequestResponse>> GetPaymentAsync(Guid id)
        {
            LastGetId = id;
            if (GetHandler != null) return GetHandler(id, new CancellationToken());

            return Task.FromResult(OperationResult<PaymentRequestResponse>.Failure(
                ErrorKind.NotFound, "not found", System.Net.HttpStatusCode.NotFound));
        }

        public Task<OperationResult<PaymentRequestResponse>> ProcessPaymentAsync(PaymentRequestCommand command)
        {
            if (ProcessHandler != null) return ProcessHandler(command, new CancellationToken());

            var resp = new PaymentRequestResponse
            {
                Id = Guid.NewGuid(),
                Amount = command.Amount,
                Currency = command.Currency,
                CardNumberLastFour = command.CardNumber[^4..],
                ExpiryMonth = command.ExpiryMonth,
                ExpiryYear = command.ExpiryYear,
                Status = PaymentStatus.Authorized
            };
            return Task.FromResult(OperationResult<PaymentRequestResponse>.Success(resp));
        }
    }
}
