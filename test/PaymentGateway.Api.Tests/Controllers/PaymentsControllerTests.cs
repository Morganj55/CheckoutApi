using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Routes;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly Random _random = new();

    //[Fact]
    //public async Task PostPaymentAsync_ValidRequest_ReturnsOKAndAddsPayment()
    //{
    //    // Arrange
    //    var postPaymentRequest = new
    //    {
    //        CardNumber = "4111111111111111",
    //        ExpiryMonth = _random.Next(1, 12),
    //        ExpiryYear = _random.Next(2023, 2030),
    //        Cvv = "123",
    //        Amount = _random.Next(1, 10000),
    //        Currency = "GBP"
    //    };

    //    var paymentsRepository = new PaymentsRepository();
    //    var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
    //    var client = webApplicationFactory.WithWebHostBuilder(builder =>
    //        builder.ConfigureServices(services => ((ServiceCollection)services)
    //            .AddSingleton(paymentsRepository)))
    //        .CreateClient();

    //    // Act
    //    var response = await client.PostAsJsonAsync(RouteContants.PaymentsBase, postPaymentRequest);
    //    var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

    //    // Assert
    //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //    Assert.NotNull(paymentResponse);
    //    Assert.Equal(postPaymentRequest.ExpiryMonth, paymentResponse!.ExpiryMonth);
    //    Assert.Equal(postPaymentRequest.ExpiryYear, paymentResponse.ExpiryYear);
    //    Assert.Equal(postPaymentRequest.Amount, paymentResponse.Amount);
    //    Assert.Equal(postPaymentRequest.Currency, paymentResponse.Currency);
    //    Assert.Equal(postPaymentRequest.Amount, paymentResponse.Amount);
    //    Assert.Single(paymentsRepository.Payments);
    //}

    //[Fact]
    //public async Task RetrievesAPaymentSuccessfully()
    //{
    //    // Arrange
    //    var payment = new PostPaymentResponse
    //    {
    //        Id = Guid.NewGuid(),
    //        ExpiryYear = _random.Next(2023, 2030),
    //        ExpiryMonth = _random.Next(1, 12),
    //        Amount = _random.Next(1, 10000),
    //        CardNumberLastFour = _random.Next(1111, 9999).ToString(),
    //        Currency = "GBP"
    //    };

    //    var paymentsRepository = new PaymentsRepository();
    //    paymentsRepository.Add(payment);

    //    var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
    //    var client = webApplicationFactory.WithWebHostBuilder(builder =>
    //        builder.ConfigureServices(services => ((ServiceCollection)services)
    //            .AddSingleton(paymentsRepository)))
    //        .CreateClient();

    //    // Act
    //    var response = await client.GetAsync($"/api/Payments/{payment.Id}");
    //    var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

    //    // Assert
    //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //    Assert.NotNull(paymentResponse);
    //}

    [Fact]
    public async Task PostPaymentAsync_InvalidRequest_Returns400BadRequest()
    {
        // Arrange
        // Invalid CVV length
        var postPaymentRequest = new
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = _random.Next(1, 12),
            ExpiryYear = DateTime.Now.Year + 1,
            Cvv = "12345555555",
            Amount = _random.Next(1, 10000),
            Currency = "GBP"
        };

        var paymentsRepository = new PaymentsRepository();
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(RouteContants.PaymentsBase, postPaymentRequest);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}