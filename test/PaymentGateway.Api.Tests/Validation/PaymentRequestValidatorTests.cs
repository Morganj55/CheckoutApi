using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Tests.Validation
{
    public class PaymentRequestValidatorTests
    {
        private static (string card, int month, int year, string ccy, int amount, string cvv)
            MakeValid()
        {
            var next = DateTime.UtcNow.AddMonths(1);
            return ("4242424242424242", next.Month, next.Year, "USD", 100, "123");
        }

        private static ValidationResult? ErrorFor(IEnumerable<ValidationResult> results, string member) =>
            results.FirstOrDefault(r => r.MemberNames.Contains(member));

        // --- Validate(...) aggregate ----------------------------------------

        [Fact]
        public void Validate_AllValid_ReturnsNoErrors()
        {
            var (card, m, y, ccy, amt, cvv) = MakeValid();

            var results = PaymentRequestValidator.Validate(card, m, y, ccy, amt, cvv);

            Assert.Empty(results);
        }

        [Fact]
        public void Validate_MultipleInvalid_ReturnsAllErrors()
        {
            // Intentionally break several fields
            var (card, m, y, ccy, amt, cvv) = MakeValid();
            card = "123A";     // non-digit
            m = 0;             // out of range
            y = 0;             // invalid year
            ccy = "ZZZ";       // invalid ISO
            amt = 0;           // not positive
            cvv = "12";        // too short

            var results = PaymentRequestValidator.Validate(card, m, y, ccy, amt, cvv);

            Assert.True(results.Count >= 5); // exact count may vary if multiple per field
            Assert.NotNull(ErrorFor(results, nameof(PostPaymentRequest.CardNumber)));
            Assert.NotNull(ErrorFor(results, nameof(PostPaymentRequest.ExpiryMonth)));
            Assert.NotNull(ErrorFor(results, nameof(PostPaymentRequest.ExpiryYear)));
            Assert.NotNull(ErrorFor(results, nameof(PostPaymentRequest.Currency)));
            Assert.NotNull(ErrorFor(results, nameof(PostPaymentRequest.Amount)));
            Assert.NotNull(ErrorFor(results, nameof(PostPaymentRequest.Cvv)));
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateCardNumber_Required(string input)
        {
            var err = PaymentRequestValidator.ValidateCardNumber(input);
            Assert.NotNull(err);
            Assert.Equal("Card number is required.", err!.ErrorMessage);
            Assert.Contains(nameof(PostPaymentRequest.CardNumber), err.MemberNames);
        }

        [Fact]
        public void ValidateCardNumber_TooShort()
        {
            var err = PaymentRequestValidator.ValidateCardNumber("1234567890123"); // 13
            Assert.NotNull(err);
            Assert.Equal("Card number must be at least 14 digits.", err!.ErrorMessage);
        }

        [Fact]
        public void ValidateCardNumber_TooLong()
        {
            var err = PaymentRequestValidator.ValidateCardNumber("12345678901234567890"); // 20
            Assert.NotNull(err);
            Assert.Equal("Card number must be at most 19 digits.", err!.ErrorMessage);
        }

        [Fact]
        public void ValidateCardNumber_NonDigits()
        {
            var err = PaymentRequestValidator.ValidateCardNumber("12345678901234A");
            Assert.NotNull(err);
            Assert.Equal("Card number must contain digits only.", err!.ErrorMessage);
        }

        [Fact]
        public void ValidateCardNumber_Valid()
        {
            var err = PaymentRequestValidator.ValidateCardNumber("4242424242424242");
            Assert.Null(err);
        }

        [Fact]
        public void ValidateExpiryMonth_RequiredWhenZero()
        {
            var err = PaymentRequestValidator.ValidateExpiryMonth(0);
            Assert.NotNull(err);
            Assert.Equal("Expiry month is required.", err!.ErrorMessage);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(13)]
        public void ValidateExpiryMonth_OutOfRange(int month)
        {
            var err = PaymentRequestValidator.ValidateExpiryMonth(month);
            Assert.NotNull(err);
            Assert.Equal("Expiry month must be between 1 and 12.", err!.ErrorMessage);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(12)]
        public void ValidateExpiryMonth_Valid(int month)
        {
            var err = PaymentRequestValidator.ValidateExpiryMonth(month);
            Assert.Null(err);
        }

        [Fact]
        public void ValidateExpiryYear_RequiredWhenZero()
        {
            var err = PaymentRequestValidator.ValidateExpiryYear(0);
            Assert.NotNull(err);
            Assert.Equal("Expiry year is required.", err!.ErrorMessage);
        }

        [Fact]
        public void ValidateExpiryYear_InvalidWhenNegative()
        {
            var err = PaymentRequestValidator.ValidateExpiryYear(-1);
            Assert.NotNull(err);
            Assert.Equal("Expiry year is invalid.", err!.ErrorMessage);
        }

        [Fact]
        public void ValidateExpiryYear_ValidPositive()
        {
            var err = PaymentRequestValidator.ValidateExpiryYear(DateTime.UtcNow.Year);
            Assert.Null(err);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateCurrency_Required(string input)
        {
            var err = PaymentRequestValidator.ValidateCurrency(input);
            Assert.NotNull(err);
            Assert.Equal("Currency code is required.", err!.ErrorMessage);
        }

        [Theory]
        [InlineData("US")]
        [InlineData("USDX")]
        public void ValidateCurrency_LengthMustBe3(string input)
        {
            var err = PaymentRequestValidator.ValidateCurrency(input);
            Assert.NotNull(err);
            Assert.Equal("Currency code must be 3 characters long.", err!.ErrorMessage);
        }

        [Fact]
        public void ValidateCurrency_InvalidIso()
        {
            var err = PaymentRequestValidator.ValidateCurrency("ZZZ"); // not ISO 4217
            Assert.NotNull(err);
            Assert.Equal("Invalid currency code.", err!.ErrorMessage);
        }

        [Fact]
        public void ValidateCurrency_ValidIso()
        {
            var err = PaymentRequestValidator.ValidateCurrency("USD");
            Assert.Null(err);
        }


        [Fact]
        public void ValidateAmount_RequiredWhenZero()
        {
            var err = PaymentRequestValidator.ValidateAmount(0);
            Assert.NotNull(err);
            Assert.Equal("Payment amount is required.", err!.ErrorMessage);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void ValidateAmount_MustBePositive(int amount)
        {
            var err = PaymentRequestValidator.ValidateAmount(amount);
            Assert.NotNull(err);
            Assert.Equal("The amount must be a positive number (at least 1 minor unit).", err!.ErrorMessage);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void ValidateAmount_Valid(int amount)
        {
            var err = PaymentRequestValidator.ValidateAmount(amount);
            Assert.Null(err);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateCvv_Required(string input)
        {
            var err = PaymentRequestValidator.ValidateCvv(input);
            Assert.NotNull(err);
            Assert.Equal("Cvv is required.", err!.ErrorMessage);
        }

        [Theory]
        [InlineData("12")]
        [InlineData("12345")]
        public void ValidateCvv_LengthInvalid(string input)
        {
            var err = PaymentRequestValidator.ValidateCvv(input);
            Assert.NotNull(err);
            Assert.Equal("CVV must be 3 or 4 digits.", err!.ErrorMessage);
        }

        [Theory]
        [InlineData("12A")]
        [InlineData("A234")]
        public void ValidateCvv_MustBeNumeric(string input)
        {
            var err = PaymentRequestValidator.ValidateCvv(input);
            Assert.NotNull(err);
            Assert.Equal("CVV must only contain numeric characters.", err!.ErrorMessage);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("1234")]
        public void ValidateCvv_Valid(string input)
        {
            var err = PaymentRequestValidator.ValidateCvv(input);
            Assert.Null(err);
        }

        // --- ExpiryDateInFuture (cross-field) --------------------------------

        [Fact]
        public void ValidateExpiryDateInFuture_PastDate_Fails()
        {
            var past = DateTime.UtcNow.AddMonths(-2);
            var err = PaymentRequestValidator.ValidateExpiryDateInFuture(past.Month, past.Year);

            Assert.NotNull(err);
            Assert.Equal("The expiry date must be in the future.", err!.ErrorMessage);
            Assert.Contains(nameof(PostPaymentRequest.ExpiryMonth), err.MemberNames);
            Assert.Contains(nameof(PostPaymentRequest.ExpiryYear), err.MemberNames);
        }

        [Fact]
        public void ValidateExpiryDateInFuture_NextMonth_Passes()
        {
            var next = DateTime.UtcNow.AddMonths(1);
            var err = PaymentRequestValidator.ValidateExpiryDateInFuture(next.Month, next.Year);

            Assert.Null(err);
        }

        [Fact]
        public void ValidateExpiryDateInFuture_ShortCircuitsWhenMonthInvalid()
        {
            // Month invalid → method returns null (doesn't double-report)
            var err = PaymentRequestValidator.ValidateExpiryDateInFuture(0, DateTime.UtcNow.Year);
            Assert.Null(err);
        }

        [Fact]
        public void ValidateExpiryDateInFuture_ShortCircuitsWhenYearInvalid()
        {
            var err = PaymentRequestValidator.ValidateExpiryDateInFuture(12, 0);
            Assert.Null(err);
        }
    }
}
