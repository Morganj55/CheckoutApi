using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Clients
{
    /// <summary>
    /// Represents a client for interacting with the ExampleBank acquiring bank API to process payment requests.
    /// </summary>
    /// <remarks>This class provides functionality to send payment requests to the ExampleBank acquiring bank
    /// and retrieve the corresponding responses. It implements the <see cref="IAcquiringBankClient"/> interface,
    /// ensuring compatibility with systems that rely on this abstraction.</remarks>
    public class ExampleBank : IAcquiringBankClient
    {
        #region Fields

        private readonly HttpClient _httpClient;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleBank"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance used for making API calls.</param>
        public ExampleBank(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #endregion

        #region IAcquiringBankClient Methods

        /// <summary>
        /// Processes a payment request asynchronously by sending it to the **ExampleBank** acquiring bank.
        /// </summary>
        /// <param name="request">The payment request details encapsulated in a <see cref="PaymentRequestCommand"/>.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an 
        /// <see cref="OperationResult{T}"/> indicating success or failure, and including a 
        /// <see cref="PostBankResponse"/> on success.
        /// </returns>
        public async Task<OperationResult<PostBankResponse>> ProcessPaymentAsync(PaymentRequestCommand request, CancellationToken ct = default)
        {
            // Setup the http request
            using var httpReq = new HttpRequestMessage(HttpMethod.Post, BankOptions.PaymentRoute)
            {
                Content = new StringContent(request.ToJson(), System.Text.Encoding.UTF8, "application/json")
            };

            // Send the request
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

            // This throws an exception which will be caught by the global exception handler middleware
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<ExampleBankDto>(json);
            return OperationResult<PostBankResponse>.Success(new PostBankResponse
            {
                Authorized = data.Authorized,
                AuthorizationCode = data.AuthorizationCode
            });
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Data Transfer Object (DTO) used for deserializing the JSON response from the **ExampleBank** API.
        /// </summary>
        private class ExampleBankDto
        {
            /// <summary>
            /// Gets or sets a value indicating whether the payment was authorized.
            /// </summary>
            [JsonPropertyName("authorized")]
            public bool Authorized { get; set; }

            /// <summary>
            /// Gets or sets the unique authorization code provided by the bank.
            /// </summary>
            [JsonPropertyName("authorization_code")]
            public string AuthorizationCode { get; set; }
        }

        #endregion
    }
}
