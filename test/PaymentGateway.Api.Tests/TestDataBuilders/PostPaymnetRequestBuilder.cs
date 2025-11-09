using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Tests.TestDataBuilders
{
    public static class PostPaymnetRequestBuilder
    {
        private static string _cardNumber = "4111111111111111";
        private static int _expiryMonth = DateTime.UtcNow.AddMonths(1).Month;
        private static int _expiryYear = DateTime.UtcNow.AddYears(1).Year;
        private static string _cvv = "123";
        private static string _currency = IsoCurrencyCodes.Codes.FirstOrDefault()!;
        private static int _amount = 1000;

        public static PostPaymentRequest Build()
        {
            return new PostPaymentRequest
            {
                CardNumber = _cardNumber, // 16 digits
                ExpiryMonth = _expiryMonth,
                ExpiryYear = _expiryYear,
                Currency = _currency,
                Amount = _amount,
                Cvv = _cvv
            };
        }
    }
}


