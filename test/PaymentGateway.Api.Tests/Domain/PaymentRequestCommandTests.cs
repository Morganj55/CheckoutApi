using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Tests.Domain
{
    public class PaymentRequestCommandTests
    {
        private static (string card, int month, int year, string currency, int amount, string cvv) MakeValid()
        {
            var next = DateTime.UtcNow.AddMonths(1);
            return ("4242424242424242", next.Month, next.Year, IsoCurrencyCodes.Codes.FirstOrDefault()!, 100, "123");
        }

        [Fact]
        public void TryCreate_WithValidValues_ReturnsTrue_AndBuildsCommand()
        {
            // Arrange valid inputs
            var (card, m, y, ccy, amt, cvv) = MakeValid();

            // Act
            var ok = PaymentRequestCommand.TryCreate(card, m, y, ccy, amt, cvv, out var cmd, out var errors);

            // Assert
            Assert.True(ok);
            Assert.NotNull(cmd);
            Assert.Empty(errors);

            // Properties propagated
            Assert.Equal(card, cmd!.CardNumber);
            Assert.Equal($"{m:D2}/{y:D4}", cmd.ExpiryDate);
            Assert.Equal(ccy, cmd.Currency);
            Assert.Equal(amt, cmd.Amount);
            Assert.Equal(cvv, cmd.Cvv);
        }

        [Fact]
        public void TryCreate_WithMultipleInvalidValues_ReturnsFalse_AndErrors()
        {
            // Arrange
            var (card, m, y, ccy, amt, cvv) = MakeValid();

            // Break several fields
            card = "1234A"; // non-digit & too short
            m = 0;          // invalid month
            y = 0;          // invalid year
            ccy = "ZZZ";    // not ISO
            amt = 0;        // not positive
            cvv = "12";     // too short

            // Act
            var ok = PaymentRequestCommand.TryCreate(card, m, y, ccy, amt, cvv, out var cmd, out var errors);

            // Assert
            Assert.False(ok);
            Assert.Null(cmd);
            Assert.NotEmpty(errors);

            // Spot-check some member mappings
            Assert.Contains(errors, e => e.MemberNames.Contains("CardNumber"));
            Assert.Contains(errors, e => e.MemberNames.Contains("ExpiryMonth"));
            Assert.Contains(errors, e => e.MemberNames.Contains("ExpiryYear"));
            Assert.Contains(errors, e => e.MemberNames.Contains("Currency"));
            Assert.Contains(errors, e => e.MemberNames.Contains("Amount"));
            Assert.Contains(errors, e => e.MemberNames.Contains("Cvv"));
        }

        [Fact]
        public void TryCreate_UsesExpiryFormatting_MM_YYYY()
        {
            // Arrange
            var (card, _, _, ccy, amt, cvv) = MakeValid();
            int month = 3, year = 2027;

            // Act
            var ok = PaymentRequestCommand.TryCreate(card, month, year, ccy, amt, cvv, out var cmd, out var errors);

            // Assert
            Assert.True(ok);
            Assert.Empty(errors);
            Assert.Equal("03/2027", cmd!.ExpiryDate);
        }

        [Fact]
        public void TryCreate_Fails_WhenExpiryIsInPast()
        {
            // Arrange
            var (card, _, _, ccy, amt, cvv) = MakeValid();
            var past = DateTime.UtcNow.AddMonths(-2);

            // Act
            var ok = PaymentRequestCommand.TryCreate(card, past.Month, past.Year, ccy, amt, cvv, out var cmd, out var errors);

            // Assert
            Assert.False(ok);
            Assert.Null(cmd);
            Assert.Contains(errors, e => e.ErrorMessage == "The expiry date must be in the future.");
        }

        [Fact]
        public void TryCreate_Allows_Maximum_Int_Amount()
        {
            // Act
            var (card, m, y, ccy, _, cvv) = MakeValid();

            // Arrange
            var ok = PaymentRequestCommand.TryCreate(card, m, y, ccy, int.MaxValue, cvv, out var cmd, out var errors);

            // Assert
            Assert.True(ok);
            Assert.Empty(errors);
            Assert.Equal(int.MaxValue, cmd!.Amount);
        }

        
        [Theory]
        [InlineData(1, 2025, "01/2025")]
        [InlineData(12, 2029, "12/2029")]
        [InlineData(3, 25, "03/0025")] // shows D4 year formatting for small values
        public void FormatExpiryDate_Returns_MM_Slash_YYYY(int month, int year, string expected)
        {
            // Act
            var formatted = PaymentRequestCommand.FormatExpiryDate(month, year);

            // Assert
            Assert.Equal(expected, formatted);
        }
    }
}
