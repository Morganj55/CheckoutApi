using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Tests.ModelValidation;
using PaymentGateway.Api.Tests.Stubs;
using PaymentGateway.Api.Utility;
using PaymentGateway.Api.Tests.TestDataBuilders;

namespace PaymentGateway.Api.Tests.Services
{
    public class PaymentServiceTests
    {
        //private static PaymentRequestCommand CreateValidCommand()
        //{
        //    var validRequest = PostPaymentRequestTests.CreateValidModel();
        //    PaymentRequestCommand.TryCreate(
        //        validRequest.CardNumber,
        //        validRequest.ExpiryMonth,
        //        validRequest.ExpiryYear,
        //        validRequest.Currency,
        //        validRequest.Amount,
        //        validRequest.Cvv,
        //        out var command,
        //        out var errors);

        //    return command;
        //}

        [Fact]
        public void CreateValidCommand_Works()
        {
            var cmd = PaymentRequestCommandBuilder.Build();
            Assert.NotNull(cmd);
        }

        [Fact]
        public async Task ProcessPaymentAsync_Valid_PaymentAuthorizedAndAdded()
        {
            // Arrange
            var successResponse = new PostBankResponse { AuthorizationCode = Guid.NewGuid().ToString(), Authorized = true };
            var bankClient = new StubAquiringBankClient()
            {
                Handler = _ => Task.FromResult(OperationResult<PostBankResponse>.Success(successResponse))
            };
            var paymentRepository = new StubPaymentRepository();
            var paymentService = new PaymentService(paymentRepository, bankClient);
            var validRequest = PaymentRequestCommandBuilder.Build();

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
            var bankClient = new StubAquiringBankClient()
            {
                Handler = _ => Task.FromResult(OperationResult<PostBankResponse>.Success(successResponse))
            };
            var paymentRepository = new StubPaymentRepository();
            var paymentService = new PaymentService(paymentRepository, bankClient);
            var validRequest = PaymentRequestCommandBuilder.Build();

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
            Assert.Equal(1, paymentRepository.TotalPaymentCount);
        }

        [Fact]
        public async Task ProcessPaymentAsync_Fail_UnknownError()
        {
            // Arrange
            var error = new Error(ErrorKind.Unexpected, "Unknown error", System.Net.HttpStatusCode.InternalServerError);
            var bankClient = new StubAquiringBankClient()
            {
                Handler = _ => Task.FromResult(OperationResult<PostBankResponse>.Failure(error))
            };

            var paymentRepository = new PaymentsRepository();
            var paymentService = new PaymentService(paymentRepository, bankClient);
            var validRequest = PaymentRequestCommandBuilder.Build();

            // Act
            var res = await paymentService.ProcessPaymentAsync(validRequest);

            // Assert
            Assert.True(res.IsFailure);
            Assert.Equal(ErrorKind.Unexpected, res.Error!.Kind);
            Assert.Equal("Unknown error", res.Error.Message);
            Assert.Equal(HttpStatusCode.InternalServerError, res.Error.Code);
        }

        [Fact]
        public async Task When_Add_Fails_Returns_Unexpected_Failure()
        {
            var repo = new StubPaymentRepository
            {
                AddHandler = _ => OperationResult<bool>.Failure(ErrorKind.Unexpected, "Fail to add", null)
            };
            var bank = new StubAquiringBankClient();

            var payService = new PaymentService(repo, bank);

            var cmd = PaymentRequestCommandBuilder.Build();
            var result = await payService.ProcessPaymentAsync(cmd);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorKind.Unexpected, result.Error!.Kind);
            Assert.Equal("Could not add payment", result.Error.Message); // from your method
            Assert.Equal(1, repo.AddCalls);
            Assert.Equal(0, repo.UpdateCalls);
        }

        [Fact]
        public async Task When_Bank_Fails_Propagates_Error()
        {
            var repo = new StubPaymentRepository(); // default add succeeds
            var bank = new StubAquiringBankClient
            {
                Handler = _ => Task.FromResult(OperationResult<PostBankResponse>.Failure(
                    ErrorKind.Transient, "bank-down", HttpStatusCode.ServiceUnavailable))
            };
            var payService = new PaymentService(repo, bank);

            var cmd = PaymentRequestCommandBuilder.Build();
            var result = await payService.ProcessPaymentAsync(cmd);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorKind.Transient, result.Error!.Kind);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, result.Error.Code);
            Assert.Equal(1, repo.AddCalls);
            Assert.Equal(0, repo.UpdateCalls);
        }

        [Fact]
        public async Task When_Bank_Authorizes_And_Update_Succeeds_Returns_Success_With_Authorized_Status()
        {
            var repo = new StubPaymentRepository();
            var bank = new StubAquiringBankClient
            {
                Handler = _ => Task.FromResult(OperationResult<PostBankResponse>.Success(
                    new PostBankResponse { Authorized = true, AuthorizationCode = "OK-1" }))
            };
            var sut = new PaymentService(repo, bank);

            var cmd = PaymentRequestCommandBuilder.Build();
            var result = await sut.ProcessPaymentAsync(cmd);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(PaymentStatus.Authorized, result.Data!.Status);
            Assert.Equal(1, repo.UpdateCalls);
            Assert.True(repo.LastUpdateArgs.HasValue);
            Assert.Equal(PaymentStatus.Authorized, repo.LastUpdateArgs!.Value.status);
        }

        [Fact]
        public async Task When_Bank_Authorizes_But_Update_Fails_Returns_Transient_Accepted()
        {
            var repo = new StubPaymentRepository
            {
                UpdateHandler = (_, __) => OperationResult<PaymentRequestResponse>.Failure(
                    ErrorKind.Unexpected, "update-fail", HttpStatusCode.InternalServerError)
            };
            var bank = new StubAquiringBankClient
            {
                Handler = _ => Task.FromResult(OperationResult<PostBankResponse>.Success(
                    new PostBankResponse { Authorized = true, AuthorizationCode = "OK-2" }))
            };
            var sut = new PaymentService(repo, bank);

            var cmd = PaymentRequestCommandBuilder.Build();
            var result = await sut.ProcessPaymentAsync(cmd);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorKind.Transient, result.Error!.Kind);
            Assert.Equal(HttpStatusCode.Accepted, result.Error.Code);
            Assert.Equal(1, repo.UpdateCalls);
        }
    }
}
