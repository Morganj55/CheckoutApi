using System.ComponentModel.DataAnnotations;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace PaymentGateway.Api.Middleware
{
    /// <summary>
    /// A global exception handling middleware that catches unhandled exceptions in the pipeline,
    /// logs them, and returns a standardized **Problem Details (RFC 7807)** response to the client.
    /// </summary>
    public sealed class GlobalExceptionMiddleware
    {
        #region Fields

        /// <summary>
        /// The next middleware in the request pipeline.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Logger for recording exceptions and warning messages.
        /// </summary>
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        /// <summary>
        /// Provides information about the web hosting environment (e.g., Development, Production).
        /// </summary>
        private readonly IHostEnvironment _env;

        /// <summary>
        /// The JSON serializer options used for writing the Problem Details response.
        /// </summary>
        private readonly JsonSerializerOptions _json;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalExceptionMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next request delegate in the pipeline.</param>
        /// <param name="logger">The logger instance for this middleware.</param>
        /// <param name="env">The host environment information.</param>
        /// <param name="jsonOptions">The JSON serialization options.</param>
        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env,
            IOptions<JsonOptions> jsonOptions
        )
        {
            _next = next;
            _logger = logger;
            _env = env;
            _json = jsonOptions.Value.JsonSerializerOptions;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Invokes the middleware, calling the next delegate and catching any exceptions that occur.
        /// </summary>
        /// <param name="ctx">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
            {
                // Client canceled; usually don't log as error
                ctx.Response.Clear();
                ctx.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
                await WriteProblem(ctx, new ProblemDetails
                {
                    Status = StatusCodes.Status499ClientClosedRequest,
                    Title = "Request canceled",
                    Detail = "The client canceled the request."
                });
            }
            catch (HttpRequestException nex)
            {
                // Downstream call failed (e.g., bank API) – treat as transient 503
                _logger.LogError(nex, "Downstream HTTP error");
                ctx.Response.Clear();
                ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await WriteProblem(ctx, new ProblemDetails
                {
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Title = "Upstream service unavailable",
                    Detail = _env.IsDevelopment() ? nex.Message : "A dependent service is unavailable."
                });
            }
            catch (Exception ex)
            {
                // Last-resort handler
                _logger.LogError(ex, "Unhandled exception");

                ctx.Response.Clear();
                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;

                await WriteProblem(ctx, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred",
                    Detail = _env.IsDevelopment() ? ex.ToString() : "Please try again later."
                });
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Writes the standardized RFC 7807 Problem Details response to the HTTP context response body.
        /// </summary>
        /// <param name="ctx">The <see cref="HttpContext"/> for the current request.</param>
        /// <param name="problem">The <see cref="ProblemDetails"/> object containing error information.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
        private async Task WriteProblem(HttpContext ctx, ProblemDetails problem)
        {
            problem.Type ??= "about:blank";
            problem.Instance = ctx.TraceIdentifier;

            ctx.Response.ContentType = "application/problem+json";
            await JsonSerializer.SerializeAsync(ctx.Response.Body, problem, _json, ctx.RequestAborted);
        }

        #endregion
    }
}
