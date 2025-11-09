using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace PaymentGateway.Api.Middleware
{
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;
        private readonly JsonSerializerOptions _json;

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
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation failed");
                ctx.Response.Clear();
                ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
                await WriteProblem(ctx, new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation error",
                    Detail = vex.Message
                });
            }
            catch (UnauthorizedAccessException uax)
            {
                _logger.LogWarning(uax, "Unauthorized access");
                ctx.Response.Clear();
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                await WriteProblem(ctx, new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden",
                    Detail = "You do not have permission to perform this action."
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

        private async Task WriteProblem(HttpContext ctx, ProblemDetails problem)
        {
            problem.Type ??= "about:blank";
            problem.Instance = ctx.TraceIdentifier;

            ctx.Response.ContentType = "application/problem+json";
            await JsonSerializer.SerializeAsync(ctx.Response.Body, problem, _json, ctx.RequestAborted);
        }
    }
}
