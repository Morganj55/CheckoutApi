using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests.ModelValidation
{
    public class PostPaymentRequestTests
    {
        /// <summary>
        /// Creates a PostPaymentRequest model that is pre-filled with
        /// valid data. This is the "happy path" base for all tests.
        /// </summary>
        public static PostPaymentRequest CreateValidModel()
        {
            var futureDate = DateTime.UtcNow.AddMonths(6);
            return new PostPaymentRequest
            {
                CardNumber = "1234567890123456", // 16 digits
                ExpiryMonth = futureDate.Month,
                ExpiryYear = futureDate.Year,
                Currency = "USD",
                Amount = 1050, 
                Cvv = "123" 
            };
        }

        /// <summary>
        /// Helper to run all validations (Attributes and IValidatableObject)
        /// on the model, just like the ASP.NET Core framework does.
        /// </summary>
        /// <param name="model">The model instance to validate.</param>
        /// <returns>A list of validation errors.</returns>
        private List<ValidationResult> ValidateModel(PostPaymentRequest model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);

            Validator.TryValidateObject(model, context, validationResults, true);

            return validationResults;
        }

        /// <summary>
        /// Helper to find a specific error message for a given property.
        /// </summary>
        private ValidationResult GetError(List<ValidationResult> results, string propertyName)
        {
            return results.FirstOrDefault(v => v.MemberNames.Contains(propertyName));
        }

        [Fact]
        public void PostPaymentRequest_Valid_ValidRequest_Pass()
        {
            // Arrange
            var model = CreateValidModel();

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("1234567890123")]
        [InlineData("12345678901234567890")] 
        [InlineData("123456789012345A")]
        public void PostPaymentRequest_Invalid_CardNumber_Fail(string cardNumber)
        {
            // Arrange
            var model = CreateValidModel();
            model.CardNumber = cardNumber;

            // Act
            var results = ValidateModel(model);

            // Assert
            var error = GetError(results, nameof(model.CardNumber));
            Assert.NotNull(error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        public void PostPaymentRequest_Invalid_ExpiryMonth_Fail(int month)
        {
            // Arrange
            var model = CreateValidModel();
            model.ExpiryMonth = month;

            // Set year to something valid so we only test the month's [Range]
            model.ExpiryYear = DateTime.UtcNow.Year + 1;

            // Act
            var results = ValidateModel(model);

            // Assert
            var error = GetError(results, nameof(model.ExpiryMonth));
            Assert.NotNull(error);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("12")]
        [InlineData("12345")] 
        [InlineData("12A")] 
        public void PostPaymentRequest_Invalid_Cvv_Fail(string cvv)
        {
            // Arrange
            var model = CreateValidModel();
            model.Cvv = cvv;

            // Act
            var results = ValidateModel(model);

            // Assert
            var error = GetError(results, nameof(model.Cvv));
            Assert.NotNull(error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void PostPaymentRequest_Invalid_Amount_Fail(int amount)
        {
            // Arrange
            var model = CreateValidModel();
            model.Amount = amount;

            // Act
            var results = ValidateModel(model);

            // Assert
            var error = GetError(results, nameof(model.Amount));
            Assert.NotNull(error);
        }

        [Fact]
        public void PostPaymentRequest_Invalid_ExpiryMonth_Fails()
        {
            // Arrange
            var model = CreateValidModel();
            var pastDate = DateTime.UtcNow.AddMonths(-2);
            model.ExpiryMonth = pastDate.Month;
            model.ExpiryYear = pastDate.Year;

            // Act
            var results = ValidateModel(model);

            // Assert
            var error = Assert.Single(results);
            Assert.Contains(nameof(model.ExpiryMonth), error.MemberNames);
            Assert.Contains(nameof(model.ExpiryYear), error.MemberNames);
        }

        [Fact]
        public void PostPaymentRequest_Valid_ExpiryDate_PassesForNextMonth()
        {
            // Arrange
            var model = CreateValidModel();
            var nextMonth = DateTime.UtcNow.AddMonths(1);
            model.ExpiryMonth = nextMonth.Month;
            model.ExpiryYear = nextMonth.Year;

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results);
        }
    }
}
