using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GBLive.Tests.GiantBomb
{
    public class TestHttpClient : HttpClient
    {
        public HttpStatusCode StatusCode { get; } = HttpStatusCode.NotImplemented;
        public HttpContent Content { get; } = null;
        public Uri RequestedUri { get; set; } = null;

        public TestHttpClient(HttpStatusCode statusCode, HttpContent content)
            : this(statusCode, content, new HttpClientHandler(), true)
        { }

        public TestHttpClient(HttpStatusCode statusCode, HttpContent content, HttpMessageHandler messageHandler, bool disposeHandler)
            : base(messageHandler, disposeHandler)
        {
            StatusCode = statusCode;
            Content = content;
        }

        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestedUri = request.RequestUri;

            return Task.FromResult(new HttpResponseMessage(StatusCode) { Content = Content });
        }
    }
}
