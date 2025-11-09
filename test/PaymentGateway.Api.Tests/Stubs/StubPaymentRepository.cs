using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Tests.Stubs
{
    public class StubPaymentRepository : IPaymentRepository
    {
        public Func<PaymentRequestResponse, OperationResult<bool>>? AddHandler { get; set; }
        public Func<Guid, Task<OperationResult<PaymentRequestResponse>>>? GetHandler { get; set; }
        public Func<Guid, PaymentStatus, OperationResult<PaymentRequestResponse>>? UpdateHandler { get; set; }

        public (Guid id, PaymentStatus status)? LastUpdateArgs { get; private set; }

        public int AddCalls { get; private set; }
        public int UpdateCalls { get; private set; }

        public StubPaymentRepository()
        {
            Payments = new();
        }

        ConcurrentDictionary<Guid, PaymentRequestResponse> Payments { get; }

        public int TotalPaymentCount => Payments.Count;

        public async Task<OperationResult<bool>> Add(PaymentRequestResponse payment)
        {
            AddCalls++;
            if (AddHandler != null) return AddHandler(payment);

            return Payments.TryAdd(payment.Id, payment)
                ? OperationResult<bool>.Success(true)
                : OperationResult<bool>.Failure(ErrorKind.Unexpected, "Could not add payment", null);
        }

        public Task<OperationResult<PaymentRequestResponse>> GetAsync(Guid id)
        {
            if (GetHandler != null) return GetHandler(id);

            if (Payments.TryGetValue(id, out var val))
                return Task.FromResult(OperationResult<PaymentRequestResponse>.Success(val));

            return Task.FromResult(OperationResult<PaymentRequestResponse>.Failure(
                ErrorKind.NotFound, "Payment not found.", HttpStatusCode.NotFound));
        }

        public async Task<OperationResult<PaymentRequestResponse>> UpdatePaymentStatus(Guid id, PaymentStatus status)
        {
            UpdateCalls++;
            LastUpdateArgs = (id, status);

            if (UpdateHandler != null) return UpdateHandler(id, status);

            if (Payments.TryGetValue(id, out var updated))
            {
                updated.Status = status;
                Payments[id] = updated;
                return OperationResult<PaymentRequestResponse>.Success(updated);
            }

            return OperationResult<PaymentRequestResponse>.Failure(
                ErrorKind.NotFound, "Payment not found.", HttpStatusCode.NotFound);
        }
    }
}
