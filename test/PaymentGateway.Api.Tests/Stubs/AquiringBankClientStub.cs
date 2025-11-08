using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests.Services
{
    internal class AquiringBankClientStub : IAquiringBankClient
    {
        private readonly PostBankResponse _response;

        public AquiringBankClientStub(PostBankResponse response)
        {
            _response = response;
        }

        public async Task<PostBankResponse> ProcessPaymentAsync(PaymentRequestCommand request)
        {
            return await Task.FromResult(_response);
        }
    }
}
