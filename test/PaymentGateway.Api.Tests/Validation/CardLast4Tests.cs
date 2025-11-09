using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Tests.Validation
{
    public class CardLast4Tests
    {
        [Theory]
        [InlineData("4242424242424242", "4242")]            // plain digits
        [InlineData(" 4242 4242-4242 1234 ", "1234")]       // spaces & dashes
        [InlineData("00001234", "1234")]                    // leading zeros
        [InlineData("abcd1234", "1234")]                    // non-digits ignored
        [InlineData("12 34", "1234")]                       // minimal 4 digits with space
        [InlineData("123456", "3456")]                      // takes the rightmost four
        public void Extract_ValidInputs_ReturnsLast4(string input, string expectedLast4)
        {
            var last4 = CardLast4Extractor.Extract(input); 
            Assert.Equal(expectedLast4, last4);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Extract_NullOrWhitespace_ThrowsArgumentException(string bad)
        {
            var ex = Assert.Throws<ArgumentException>(() => CardLast4Extractor.Extract(bad!));
            Assert.Equal("cardNum", ex.ParamName);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("123")]     // fewer than 4 total digits
        [InlineData("abc")]     // zero digits after filtering
        [InlineData("--- ")]    // no digits at all
        public void Extract_FewerThanFourDigits_ThrowsArgumentException(string bad)
        {
            var ex = Assert.Throws<ArgumentException>(() => CardLast4Extractor.Extract(bad));
            Assert.Equal("cardNum", ex.ParamName);
            Assert.Contains("fewer than 4 digits", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
