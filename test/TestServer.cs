using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;

namespace GBLive.Tests
{
    public class TestServer : IDisposable
    {
        private readonly IWebHost webHost;

        public int Port { get; }
        public string Url { get; }
        public IDictionary<string, string> Samples { get; } = new Dictionary<string, string>();

        public TestServer(int port, string url)
        {
            if (port <= 1024 || port > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "port must be >1024 and <=65535");
            }

            if (String.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url), "url was not valid");
            }

            Port = port;
            Url = url;
            
            webHost = CreateWebHost();
        }

        private IWebHost CreateWebHost()
        {
            return new WebHostBuilder()
                .UseUrls($"http://{Url}:{Port}")
                .UseKestrel()
                .Configure(app =>
                    {
                        app.Run(async ctx =>
                            {
                                string path = ctx.Request.Path.Value.Substring(1); // remove the forward slash

                                int statusCode = -1;
                                string contentType = string.Empty;
                                byte[] buffer = Array.Empty<byte>();

                                if (Samples.ContainsKey(path))
                                {
                                    statusCode = 200;
                                    contentType = "application/json; charset=utf-8";

                                    buffer = Encoding.UTF8.GetBytes(Samples[path]);
                                }
                                else
                                {
                                    statusCode = 404;
                                    contentType = "text/plain";

                                    buffer = Encoding.UTF8.GetBytes("no uri here");
                                }

                                ctx.Response.StatusCode = statusCode;
                                ctx.Response.ContentType = contentType;

                                await ctx.Response.Body.WriteAsync(buffer).ConfigureAwait(false);
                            }
                        );
                    }
                )
                .Build();
        }

        public Task StartAsync() => webHost.StartAsync();

        public Task StopAsync() => webHost.StopAsync();


        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    webHost.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
