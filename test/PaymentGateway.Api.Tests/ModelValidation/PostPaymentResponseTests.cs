using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Tests.ModelValidation
{
    public class PostPaymentResponseTests
    {
        private static readonly Guid _id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        private const int _expiryMonth = 12;
        private const int _expiryYear = 2099;
        private const string _currency = "GBP";
        private const int _amountMinorUnits = 1050;

        [Fact]
        public void Ctor_WithValidArgs_SetsProperties()
        {
            // arrange
            var last4 = "4242";
            var status = PaymentStatus.Authorized;

            // act
            var dto = new PostPaymentResponse(
                id: _id,
                status: status,
                cardNumberLastFour: last4,
                expiryMonth: _expiryMonth,
                expiryYear: _expiryYear,
                currency: _currency,
                amount: _amountMinorUnits);

            // assert
            Assert.Equal(_id, dto.Id);
            Assert.Equal(status, dto.Status);
            Assert.Equal(last4, dto.CardNumberLastFour);
            Assert.Equal(_expiryMonth, dto.ExpiryMonth);
            Assert.Equal(_expiryYear, dto.ExpiryYear);
            Assert.Equal(_currency, dto.Currency);
            Assert.Equal(_amountMinorUnits, dto.Amount);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("123")]     // too short
        [InlineData("12345")]   // too long
        [InlineData("12a4")]    // non-digit
        public void Ctor_InvalidLast4_ThrowsArgumentException(string badLast4)
        {
            // act
            var ex = Assert.Throws<ArgumentException>(() =>
                new PostPaymentResponse(
                    id: _id,
                    status: PaymentStatus.Declined,
                    cardNumberLastFour: badLast4!,
                    expiryMonth: _expiryMonth,
                    expiryYear: _expiryYear,
                    currency: _currency,
                    amount: _amountMinorUnits));

            // assert
            Assert.Equal("cardNumberLastFour", ex.ParamName);
            // optional: ensure message mentions the rule
            Assert.Contains("4 digits", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
