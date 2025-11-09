using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Tests.Stubs;
using PaymentGateway.Api.Tests.TestDataBuilders;

namespace PaymentGateway.Api.Tests.Clients
{
    public class ExampleBankTests
    {
        [Fact]
        public async Task ProcessPaymentAsync_OddLastDigit_ReturnsAuthorizedWithCode()
        {
            // Arrange: simulator returns 200 authorized with auth code for odd-ending card
            var handler = new StubHttpMessageHandler
            {
                Handler = async (req, ct) =>
                {
                    Assert.Equal(HttpMethod.Post, req.Method);
                    Assert.Equal("/payments", req.RequestUri!.AbsolutePath);

                    var body = await req.Content!.ReadAsStringAsync(ct);
                
                    var bankJson = """{ "authorized": true, "authorization_code": "AUTH123" }""";
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent(bankJson, System.Text.Encoding.UTF8, "application/json")
                    };
                }
            };

            var http = new HttpClient(handler) { BaseAddress = new Uri("https://simulator.bank") };
            var client = new ExampleBank(http);
            var cmd = PaymentRequestCommandBuilder.Build('3');

            // Act
            var result = await client.ProcessPaymentAsync(cmd);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data!.Authorized);
            Assert.Equal("AUTH123", result.Data!.AuthorizationCode);
        }

        [Fact]
        public async Task ProcessPaymentAsync_EvenLastDigit_ReturnsUnauthorized()
        {
            var handler = new StubHttpMessageHandler
            {
                Handler = (req, ct) =>
                {
                    var bankJson = """{ "authorized": false }""";
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(bankJson, System.Text.Encoding.UTF8, "application/json")
                    });
                }
            };

            var http = new HttpClient(handler) { BaseAddress = new Uri("https://simulator.bank") };
            var client = new ExampleBank(http);
            var cmd = PaymentRequestCommandBuilder.Build('2'); // even

            var result = await client.ProcessPaymentAsync(cmd);

            Assert.True(result.IsSuccess);
            Assert.False(result.Data!.Authorized);
            Assert.Null(result.Data!.AuthorizationCode);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ZeroLastDigit_ThrowsUnavailable()
        {
            var handler = new StubHttpMessageHandler
            {
                Handler = (req, ct) =>
                    Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    {
                        Content = new StringContent("""{"error":"bank down"}""", System.Text.Encoding.UTF8, "application/json")
                    })
            };

            var http = new HttpClient(handler) { BaseAddress = new Uri("https://simulator.bank") };
            var client = new ExampleBank(http);
            var cmd = PaymentRequestCommandBuilder.Build('0'); // zero

            var result = await client.ProcessPaymentAsync(cmd);

            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error!.Message);
        }
    }
}
