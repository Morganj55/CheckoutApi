using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;

using PaymentGateway.Api.Middleware;

namespace PaymentGateway.Api.Tests.Middleware
{
    public class GlobalExceptionMiddlewareTests
    {
        [Fact]
        public async Task When_Next_Throws_UnhandledException_Returns_Problem500()
        {
            // arrange
            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream();

            var jsonOpts = Options.Create(new JsonOptions()); // uses default STJ opts
            var env = new FakeHostEnvironment(isDevelopment: false);

            var middleware = new GlobalExceptionMiddleware(
                next: _ => throw new InvalidOperationException("boom"),
                logger: NullLogger<GlobalExceptionMiddleware>.Instance,
                env: env,
                jsonOptions: jsonOpts);

            // act
            await middleware.Invoke(ctx);

            // assert
            Assert.Equal(StatusCodes.Status500InternalServerError, ctx.Response.StatusCode);
            Assert.Equal("application/problem+json", ctx.Response.ContentType);

            ctx.Response.Body.Position = 0;
            var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(ctx.Response.Body);
            Assert.Equal(500, problem!.Status);
            Assert.Equal("An unexpected error occurred", problem.Title);
            Assert.False(string.IsNullOrWhiteSpace(problem.Instance)); // trace id
        }

        [Fact]
        public async Task When_Next_Throws_HttpRequestException_Returns_503()
        {
            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream();

            var jsonOpts = Options.Create(new JsonOptions());
            var env = new FakeHostEnvironment(isDevelopment: false);

            var middleware = new GlobalExceptionMiddleware(
                next: _ => throw new HttpRequestException("upstream down"),
                logger: NullLogger<GlobalExceptionMiddleware>.Instance,
                env: env,
                jsonOptions: jsonOpts);

            await middleware.Invoke(ctx);

            Assert.Equal(StatusCodes.Status503ServiceUnavailable, ctx.Response.StatusCode);
        }

        private sealed class FakeHostEnvironment : IHostEnvironment
        {
            public FakeHostEnvironment(bool isDevelopment) => EnvironmentName = isDevelopment ? Environments.Development : Environments.Production;
            public string EnvironmentName { get; set; }
            public string ApplicationName { get; set; } = "TestApp";
            public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        }
    }
}
