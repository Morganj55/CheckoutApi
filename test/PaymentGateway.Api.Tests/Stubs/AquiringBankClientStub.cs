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
    internal class AquiringBankClientStub : IAquiringBankClient
    {
        private readonly bool _success;
        private readonly PostBankResponse _response;
        private readonly Error _error;

        public AquiringBankClientStub(bool success, PostBankResponse response, Error error)
        {
            _success = success;
            _response = response;
            _error = error;
        }

        public async Task<OperationResult<PostBankResponse>> ProcessPaymentAsync(PaymentRequestCommand request)
        {
            if (_success)
            {
                return await Task.FromResult(OperationResult<PostBankResponse>.Success(_response));
            }
            
            return await Task.FromResult(OperationResult<PostBankResponse>.Failure(_error));

        }
    }
}
