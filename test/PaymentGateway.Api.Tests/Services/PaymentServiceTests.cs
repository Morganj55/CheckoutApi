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
using PaymentGateway.Api.Utility;

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
            var bankClient = new AquiringBankClientStub(true, successResponse, null);
            var paymentRepository = new PaymentsRepository();
            var paymentService = new PaymentService(paymentRepository, bankClient);
            var validRequest = CreateValidCommand();

            // Act
            var res = await paymentService.ProcessPaymentAsync(validRequest);

            // Assert
            Assert.NotNull(res);
            Assert.Equal(Models.PaymentStatus.Authorized, res.Data.Status);
            Assert.Equal(validRequest.Amount, res.Data.Amount);
            Assert.Equal(validRequest.Currency, res.Data.Currency);
            Assert.Equal(validRequest.ExpiryMonth, res.Data.ExpiryMonth);
            Assert.Equal(validRequest.ExpiryYear, res.Data.ExpiryYear);
            Assert.Equal(validRequest.CardNumber.Substring(validRequest.CardNumber.Length - 4, 4), res.Data.CardNumberLastFour);
            Assert.NotEqual(Guid.Empty, res.Data.Id);
            Assert.Equal(1, paymentRepository.TotalPaymentCount);
        }

        [Fact]
        public async Task ProcessPaymentAsync_Valid_PaymentDeclined()
        {
            // Arrange
            var successResponse = new PostBankResponse { AuthorizationCode = Guid.NewGuid().ToString(), Authorized = false };
            var bankClient = new AquiringBankClientStub(true, successResponse, null);
            var paymentRepository = new PaymentsRepository();
            var paymentService = new PaymentService(paymentRepository, bankClient);
            var validRequest = CreateValidCommand();

            // Act
            var res = await paymentService.ProcessPaymentAsync(validRequest);

            // Assert
            Assert.True(res.IsSuccess);
            Assert.NotNull(res.Data);
            Assert.Equal(Models.PaymentStatus.Declined, res.Data.Status);
            Assert.Equal(validRequest.Amount, res.Data.Amount);
            Assert.Equal(validRequest.Currency, res.Data.Currency);
            Assert.Equal(validRequest.ExpiryMonth, res.Data.ExpiryMonth);
            Assert.Equal(validRequest.ExpiryYear, res.Data.ExpiryYear);
            Assert.Equal(validRequest.CardNumber[^4..], res.Data.CardNumberLastFour);
            Assert.NotEqual(Guid.Empty, res.Data.Id);
            Assert.Equal(0, paymentRepository.TotalPaymentCount);
        }

        [Fact]
        public async Task ProcessPaymentAsync_Fail_UnknownError()
        {
            // Arrange
            var error = new Error(ErrorKind.Unexpected, "Unknown error", System.Net.HttpStatusCode.InternalServerError);
            var bankClient = new AquiringBankClientStub(false, new PostBankResponse(), error);
            var paymentRepository = new PaymentsRepository();
            var paymentService = new PaymentService(paymentRepository, bankClient);
            var validRequest = CreateValidCommand();

            // Act
            var res = await paymentService.ProcessPaymentAsync(validRequest);

            // Assert
            Assert.True(res.IsFailure);
            Assert.Equal(ErrorKind.Unexpected, res.Error.Kind);
            Assert.Equal("Unknown error", res.Error.Message);
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, res.Error.Code);
        }
    }
}
