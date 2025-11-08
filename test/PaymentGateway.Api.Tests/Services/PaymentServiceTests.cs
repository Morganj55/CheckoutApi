using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Tests.ModelValidation;
using PaymentGateway.Api.Tests.Stubs;

namespace PaymentGateway.Api.Tests.Services
{
    public class PaymentServiceTests
    {
        private static PaymentRequestCommand CreateValidCommand()
        {
            var validRequest = PostPaymentRequestTests.CreateValidModel();
            PaymentRequestCommand.TryCreate(
                validRequest.CardNumber,
                validRequest.ExpiryMonth,
                validRequest.ExpiryYear,
                validRequest.Currency,
                validRequest.Amount,
                validRequest.Cvv,
                out var command,
                out var errors);

            return command;
        }

        [Fact]
        public void CreateValidCommand_Works()
        {
            var cmd = CreateValidCommand();
            Assert.NotNull(cmd);
        }

        [Fact]
        public async Task ProcessPaymentAsync_Valid_PaymentAuthorizedAndAdded()
        {
            // Arrange
            var successResponse = new PostBankResponse { AuthorizationCode = Guid.NewGuid().ToString(), Authorized = true };
            var bankClient = new AquiringBankClientStub(successResponse);
            var paymentRepository = new PaymentsRepository();
            var paymentService = new PaymentService(paymentRepository, bankClient);
            var validRequest = CreateValidCommand();

            // Act
            var res = await paymentService.ProcessPaymentAsync(validRequest);

            // Assert
            Assert.NotNull(res);
            Assert.Equal(Models.PaymentStatus.Authorized, res.Status);
            Assert.Equal(validRequest.Amount, res.Amount);
            Assert.Equal(validRequest.Currency, res.Currency);
            Assert.Equal(validRequest.ExpiryMonth, res.ExpiryMonth);
            Assert.Equal(validRequest.ExpiryYear, res.ExpiryYear);
            Assert.Equal(validRequest.CardNumber.Substring(validRequest.CardNumber.Length - 4, 4), res.CardNumberLastFour);
            Assert.NotEqual(Guid.Empty, res.Id);
            Assert.Equal(1, paymentRepository.TotalPaymentCount);
        }

        [Fact]
        public async Task ProcessPaymentAsync_Valid_PaymentDeclined()
        {
            // Arrange
            var successResponse = new PostBankResponse { AuthorizationCode = Guid.NewGuid().ToString(), Authorized = false };
            var bankClient = new AquiringBankClientStub(successResponse);
            var paymentRepository = new PaymentsRepository();
            var paymentService = new PaymentService(paymentRepository, bankClient);
            var validRequest = CreateValidCommand();

            // Act
            var res = await paymentService.ProcessPaymentAsync(validRequest);

            // Assert
            Assert.NotNull(res);
            Assert.Equal(Models.PaymentStatus.Declined, res.Status);
            Assert.Equal(validRequest.Amount, res.Amount);
            Assert.Equal(validRequest.Currency, res.Currency);
            Assert.Equal(validRequest.ExpiryMonth, res.ExpiryMonth);
            Assert.Equal(validRequest.ExpiryYear, res.ExpiryYear);
            Assert.Equal(validRequest.CardNumber.Substring(validRequest.CardNumber.Length - 4, 4), res.CardNumberLastFour);
            Assert.NotEqual(Guid.Empty, res.Id);
            Assert.Equal(0, paymentRepository.TotalPaymentCount);
        }
    }
}
