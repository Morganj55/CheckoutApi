using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Tests.TestDataBuilders
{
    public static class PaymentRequestCommandBuilder
    {
        private static string _card = "1234567890123456";
        private static int _year = DateTime.UtcNow.AddYears(1).Year;
        private static int _month = DateTime.UtcNow.AddMonths(1).Month;
        private static string _currency = IsoCurrencyCodes.Codes.FirstOrDefault()!;
        private static int _amount = 100000;
        private static string _cvv = "123";

        public static PaymentRequestCommand Build()
        {
            var ok = PaymentRequestCommand.TryCreate(_card, _month, _year, _currency, _amount, _cvv, out var cmd, out var errors);
            if (!ok) throw new InvalidOperationException($"Builder produced invalid command: {errors}");
            return cmd!;
        }

        public static PaymentRequestCommand Build(char endNumber)
        {
            var cardNumber = _card;
            cardNumber = cardNumber[..^1] + endNumber;
            var ok = PaymentRequestCommand.TryCreate(cardNumber, _month, _year, _currency, _amount, _cvv, out var cmd, out var errors);
            if (!ok) throw new InvalidOperationException($"Builder produced invalid command: {errors}");
            return cmd!;
        }
    }
}
