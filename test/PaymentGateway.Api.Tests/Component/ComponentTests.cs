using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Tests.Stubs;
using PaymentGateway.Api.Tests.TestDataBuilders;
using PaymentGateway.Api.Utility;


namespace PaymentGateway.Api.Tests.Component
{
    public class PaymentsComponentTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public PaymentsComponentTests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task PostPayment_Authorized_Returns_200_And_Body()
        {
            // Arrange
            _factory.StubBank.Handler = _ =>
                Task.FromResult(OperationResult<PostBankResponse>.Success(
                    new PostBankResponse { Authorized = true, AuthorizationCode = "OK-123" }));

            using var client = _factory.CreateClient();
            var req = PostPaymnetRequestBuilder.Build();

            // Act
            var resp = await client.PostAsJsonAsync("/api/v1/payments", req);

            // Assert
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var body = await resp.Content.ReadFromJsonAsync<PostPaymentResponse>();
            Assert.NotNull(body);
            Assert.Equal("1111", body!.CardNumberLastFour);
            Assert.Equal(PaymentStatus.Authorized, body.Status);
            Assert.Equal(req.Currency, body.Currency);
            Assert.Equal(req.ExpiryMonth, body.ExpiryMonth);
            Assert.Equal(req.ExpiryYear, body.ExpiryYear);
        }

        [Fact]
        public async Task PostPayment_BankFails_Maps_StatusCode_And_Message()
        {
            // Arrange
            _factory.StubBank.Handler = _ =>
                Task.FromResult(OperationResult<PostBankResponse>.Failure(
                    ErrorKind.Transient, "Upstream unavailable", HttpStatusCode.ServiceUnavailable));

            using var client = _factory.CreateClient();
            var req = PostPaymnetRequestBuilder.Build();

            // Act
            var resp = await client.PostAsJsonAsync("/api/v1/payments", req);

            // Assert
            Assert.Equal(HttpStatusCode.ServiceUnavailable, resp.StatusCode);
            var msg = await resp.Content.ReadAsStringAsync();
            Assert.Contains("Upstream unavailable", msg);
        }

        [Fact]
        public async Task Get_ById_WhenExists_Returns_200_WithBody()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var validResponse = PaymentRequestResponseBuilder.Build(id);
            var seededRepo = new PaymentsRepository();
            await seededRepo.Add(validResponse);

            using var app = _factory.WithWebHostBuilder(b =>
            {
                b.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IPaymentRepository>();
                    services.AddSingleton<IPaymentRepository>(seededRepo);
                });
            });

            var client = app.CreateClient();

            // Act
            var resp = await client.GetAsync($"/api/v1/payments/{id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var body = await resp.Content.ReadFromJsonAsync<GetPaymentResponse>();
            Assert.NotNull(body);
            Assert.Equal(id, body!.Id);
            Assert.Equal(validResponse.CardNumberLastFour, body.CardNumberLastFour);
            Assert.Equal(PaymentStatus.Authorized, body.Status);
        }

        [Fact]
        public async Task Get_ById_WhenMissing_Returns_404()
        {
            using var client = _factory.CreateClient();
            var resp = await client.GetAsync($"/api/v1/payments/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

    }

    public class ApiFactory : WebApplicationFactory<Program>
    {
        internal StubAquiringBankClient StubBank { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove the typed HttpClient registration for IAquiringBankClient -> ExampleBank
                services.RemoveAll<IAcquiringBankClient>();

                // Replace with your stub (singleton so we can configure it per-test)
                services.AddSingleton<IAcquiringBankClient>(StubBank);

                services.AddSingleton<IPaymentRepository, PaymentsRepository>();
                services.AddSingleton<IPaymentService, PaymentService>();

                // If your pipeline requires BankOptions from IOptions<BankOptions>, you can add a dummy:
                services.Configure<BankOptions>(o => o.BaseUrl = "https://bank.example/");
            });
        }
    }
}
