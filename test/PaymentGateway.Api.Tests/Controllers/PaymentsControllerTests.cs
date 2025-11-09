using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Routes;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Tests.Stubs;
using PaymentGateway.Api.Tests.TestDataBuilders;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Tests.Controllers;

public class PaymentsControllerTests
{
    private static PaymentsController CreateController(StubPaymentService stub)=> new PaymentsController(stub);

    [Fact]
    public async Task GetPaymentAsync_EmptyId_ReturnsBadRequest()
    {
        var stub = new StubPaymentService();
        var controller = CreateController(stub);

        var result = await controller.GetPaymentAsync(Guid.Empty);

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, bad.StatusCode);
        Assert.Equal("Invalid payment ID.", bad.Value);
        Assert.Null(stub.LastGetId);
    }

    [Fact]
    public async Task GetPaymentAsync_Found_ReturnsOkWithBody()
    {
        var id = Guid.NewGuid();
        var stub = new StubPaymentService
        {
            GetHandler = (gid, _) =>
            {
                var body = PaymentRequestResponseBuilder.Build(gid);
                return Task.FromResult(OperationResult<PaymentRequestResponse>.Success(body));
            }
        };
        var controller = CreateController(stub);

        var result = await controller.GetPaymentAsync(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<GetPaymentResponse>(ok.Value);
        Assert.Equal(id, body.Id);
        Assert.Equal(PaymentStatus.Authorized, body.Status);
        Assert.Equal(id, stub.LastGetId);
    }

    [Fact]
    public async Task GetPaymentAsync_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        var stub = new StubPaymentService
        {
            GetHandler = (gid, _) =>
                Task.FromResult(OperationResult<PaymentRequestResponse>.Failure(
                    ErrorKind.NotFound, "nope", HttpStatusCode.NotFound))
        };
        var controller = CreateController(stub);

        var result = await controller.GetPaymentAsync(id);

        Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(id, stub.LastGetId);
    }

    [Fact]
    public async Task PostPaymentAsync_InvalidBody_ReturnsBadRequest()
    {
        var req = new PostPaymentRequest
        {
            CardNumber = "",      // invalid so TryCreate fails
            ExpiryMonth = 0,
            ExpiryYear = 2000,
            Currency = "",
            Amount = -100000,
            Cvv = ""
        };

        var stub = new StubPaymentService();
        var controller = CreateController(stub);

        var result = await controller.PostPaymentAsync(req);

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, bad.StatusCode);
    }

    [Fact]
    public async Task PostPaymentAsync_ServiceFailure_MapsStatusCodeAndMessage()
    {
        var req = new PostPaymentRequest
        {
            CardNumber = "4242424242424242",
            ExpiryMonth = 12,
            ExpiryYear = 2099,
            Currency = "GBP",
            Amount = 1111,
            Cvv = "123"
        };

        var stub = new StubPaymentService
        {
            ProcessHandler = (_, __) =>
                Task.FromResult(OperationResult<PaymentRequestResponse>.Failure(
                    ErrorKind.Transient, "Bank down", HttpStatusCode.ServiceUnavailable))
        };

        var controller = CreateController(stub);

        var result = await controller.PostPaymentAsync(req);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, obj.StatusCode);
        Assert.Equal("Bank down", obj.Value);
    }

    [Fact]
    public async Task PostPaymentAsync_ServiceSuccess_ReturnsOkWithProjectedBody()
    {
        var req = new PostPaymentRequest
        {
            CardNumber = "4242424242424242",
            ExpiryMonth = 12,
            ExpiryYear = 2099,
            Currency = "GBP",
            Amount = 11111,
            Cvv = "123"
        };

        var id = Guid.NewGuid();
        var repoModel = PaymentRequestResponseBuilder.Build(id);

        var stub = new StubPaymentService
        {
            ProcessHandler = (_, __) =>
                Task.FromResult(OperationResult<PaymentRequestResponse>.Success(repoModel))
        };

        var controller = CreateController(stub);

        var result = await controller.PostPaymentAsync(req);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<PostPaymentResponse>(ok.Value);

        Assert.Equal(id, body.Id);
        Assert.Equal(repoModel.Amount, body.Amount);
        Assert.Equal(repoModel.Currency, body.Currency);
        Assert.Equal(repoModel.CardNumberLastFour, body.CardNumberLastFour);
        Assert.Equal(repoModel.ExpiryMonth, body.ExpiryMonth);
        Assert.Equal(repoModel.ExpiryYear, body.ExpiryYear);
        Assert.Equal(repoModel.Status, body.Status);
    }
}