using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Clients
{
    /// <summary>
    /// Represents a client for interacting with the ExampleBank acquiring bank API to process payment requests.
    /// </summary>
    /// <remarks>This class provides functionality to send payment requests to the ExampleBank acquiring bank
    /// and retrieve the corresponding responses. It implements the <see cref="IAquiringBankClient"/> interface,
    /// ensuring compatibility with systems that rely on this abstraction.</remarks>
    public class ExampleBank : IAquiringBankClient
    {
        private readonly HttpClient _httpClient;

        public ExampleBank(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<OperationResult<PostBankResponse>> ProcessPaymentAsync(PaymentRequestCommand request)
        {
            try
            {
                var jsonReq = request.ToJson();

                using var httpReq = new HttpRequestMessage(HttpMethod.Post, BankOptions.PaymentRoute)
                {
                    Content = new StringContent(jsonReq, System.Text.Encoding.UTF8, "application/json")
                };

                var ct = new CancellationToken();
                using var resp = await _httpClient.SendAsync(httpReq, HttpCompletionOption.ResponseHeadersRead, ct);

                if (resp.StatusCode == HttpStatusCode.BadRequest)
                {
                    var msg = await resp.Content.ReadAsStringAsync(ct);
                    return OperationResult<PostBankResponse>.Failure(ErrorKind.Unexpected, msg, resp.StatusCode);
                }

                if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    var msg = await resp.Content.ReadAsStringAsync(ct);
                    return OperationResult<PostBankResponse>.Failure(ErrorKind.Transient, msg, resp.StatusCode);
                }

                resp.EnsureSuccessStatusCode();

                var json = await resp.Content.ReadAsStringAsync(ct);
                var data = JsonSerializer.Deserialize<ExampleBankDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new InvalidOperationException("Empty/invalid JSON from bank.");

                return OperationResult<PostBankResponse>.Success(new PostBankResponse
                {
                    Authorized = data.Authorized,
                    AuthorizationCode = data.AuthorizationCode
                });

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private class ExampleBankDto
        {
            [JsonPropertyName("authorized")]
            public bool Authorized { get; set; }

            [JsonPropertyName("authorization_code")]
            public string AuthorizationCode { get; set; }
        }

    }
}
