using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Tests.TestDataBuilders
{
    public static class PaymentRequestResponseBuilder
    {
        private static string _cardNumberLastFour = "1234";
        private static int _expiryMonth = DateTime.UtcNow.AddMonths(1).Month;
        private static int _expiryYear = DateTime.UtcNow.AddYears(1).Year;
        private static string _currency = IsoCurrencyCodes.Codes.FirstOrDefault()!;
        private static int _amount = 1000;

        public static PaymentRequestResponse Build(Guid id, PaymentStatus status = PaymentStatus.Authorized)
        {
            return new PaymentRequestResponse(id, status, _cardNumberLastFour, _expiryMonth, _expiryYear, _currency, _amount);
        }
    }
}
