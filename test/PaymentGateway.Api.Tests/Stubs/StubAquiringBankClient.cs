using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Tests.Stubs
{
    internal class StubAquiringBankClient : IAcquiringBankClient
    {
        public Func<PaymentRequestCommand, Task<OperationResult<PostBankResponse>>>? Handler { get; set; }
        public PaymentRequestCommand? LastRequest { get; private set; }

        public Task<OperationResult<PostBankResponse>> ProcessPaymentAsync(PaymentRequestCommand request, CancellationToken ct = default)
        {
            LastRequest = request;
            if (Handler != null) return Handler(request);
            // default: authorized
            return Task.FromResult(OperationResult<PostBankResponse>.Success(
                new PostBankResponse { Authorized = true, AuthorizationCode = "OK-123" }));
        }
    }
}
