using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Tests.ModelValidation
{
    public class ValidCurrencyAttributeTests
    {
        [Fact]
        public void ValidCurrencyAttribute_Valid_Pass()
        {
            // Arange
            var model = PostPaymentRequestTests.CreateValidModel();
            var ctx = new ValidationContext(model) { MemberName = nameof(model.Currency) };
            var results = new List<ValidationResult>();

            // Act
            var ok = Validator.TryValidateProperty(model.Currency, ctx, results);

            // Assert
            Assert.True(ok);
            Assert.Empty(results);
        }

        [Theory]
        [InlineData("x")]
        [InlineData("USDX")]
        [InlineData("123")]
        [InlineData(null)]
        [InlineData("")]
        public void ValidCurrencyAttribute_Invalid_CodeIncorrect_Fail(string code)
        {
            // Arange
            var model = PostPaymentRequestTests.CreateValidModel();
            model.Currency = code;
            var ctx = new ValidationContext(model) { MemberName = nameof(model.Currency) };
            var results = new List<ValidationResult>();

            // Act
            var result = Validator.TryValidateProperty(model.Currency, ctx, results);

            // Assert
            Assert.False(result); 
        }
    }
}
