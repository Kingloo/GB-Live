/*

 TestServer requires the following NuGet packages

 <PackageReference Include="Microsoft.AspNetCore.Hosting" Version="2.2.7" />
 <PackageReference Include="Microsoft.AspNetCore.Server.Kestrel" Version="2.2.0" />

*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace GBLive.Tests
{
	public class ContentTypes
	{
		public const string TextPlain = "text/plain";
		public const string JsonUTF8 = "application/json; charset=utf-8";
	}

	public class Sample
	{
		public string Path { get; set; } = string.Empty;
		public int StatusCode { get; set; } = 0;
		public string ContentType { get; set; } = string.Empty;
		public string Text { get; set; } = string.Empty;
		public bool RequireUserAgent { get; set; } = false;
	}

	public class TestServer : IDisposable
	{
		private readonly IWebHost webHost;

		public int Port { get; }
		public string Url { get; }
		public ICollection<Sample> Samples { get; } = new Collection<Sample>();

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

								Sample sample = Samples.Single(s => s.Path.Equals(path, StringComparison.OrdinalIgnoreCase));

								if (sample.RequireUserAgent && UserAgentIsMissingOrEmpty(ctx.Request.Headers))
								{
									ctx.Response.StatusCode = 404;

									return;
								}

								ctx.Response.StatusCode = sample.StatusCode;
								ctx.Response.ContentType = sample.ContentType;

								byte[] buffer = Encoding.UTF8.GetBytes(sample.Text);

								await ctx.Response.Body.WriteAsync(buffer).ConfigureAwait(false);
							}
						);
					}
				)
				.Build();
		}

		private static bool UserAgentIsMissingOrEmpty(IHeaderDictionary headers)
		{
			string userAgentHeaderName = "User-Agent";

			if (!headers.ContainsKey(userAgentHeaderName))
			{
				return false;
			}

			if (String.IsNullOrWhiteSpace(headers[userAgentHeaderName]))
			{
				return false;
			}

			return true;
		}

		public Task StartAsync() => webHost.StartAsync();

		public Task StopAsync() => webHost.StopAsync();

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
	}
}
