using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.Api.Tests.Stubs
{
    public class StubHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? Handler { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            return Handler is not null ? Handler(request, ct): Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
