using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GBLive.Options;

namespace GBLive.GiantBomb
{
	public static class Updater
	{
		private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};

		public static Task<UpcomingData?> UpdateAsync(GBLiveOptions gbLiveOptions, GiantBombOptions giantBombOptions)
			=> UpdateAsyncInternal(gbLiveOptions, giantBombOptions, CancellationToken.None);

		public static Task<UpcomingData?> UpdateAsync(GBLiveOptions gbLiveOptions, GiantBombOptions giantBombOptions, CancellationToken cancellationToken)
			=> UpdateAsyncInternal(gbLiveOptions, giantBombOptions, cancellationToken);

		private static async Task<UpcomingData?> UpdateAsyncInternal(
			GBLiveOptions gbLiveOptions,
			GiantBombOptions giantBombOptions,
			CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(gbLiveOptions);
			ArgumentNullException.ThrowIfNull(giantBombOptions);
			ArgumentNullException.ThrowIfNull(giantBombOptions.UpcomingJsonUri);

			UpcomingData? upcomingData = null;

			HttpMessageHandler handler = new SocketsHttpHandler
			{
				AllowAutoRedirect = false,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
				MaxConnectionsPerServer = 1,
				SslOptions = new SslClientAuthenticationOptions
				{
					AllowRenegotiation = false,
#pragma warning disable CA5398 // I want to set these explicitly
					EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
#pragma warning restore CA5398
					EncryptionPolicy = EncryptionPolicy.RequireEncryption
				}
			};

			HttpClient client = new HttpClient(handler, disposeHandler: false);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, giantBombOptions.UpcomingJsonUri)
			{
				Version = HttpVersion.Version20
			};

			request.Headers.Accept.ParseAdd("application/json; charset=utf-8");
			request.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
			request.Headers.Connection.ParseAdd("close"); // Connection is not used under HTTP/2, but the worst they can do is ignore it
			request.Headers.Host = "www.giantbomb.com"; // this MUST have www - no idea why

			if (!String.IsNullOrWhiteSpace(gbLiveOptions.UserAgent))
			{
				request.Headers.UserAgent.ParseAdd(gbLiveOptions.UserAgent);
			}

			HttpResponseMessage? response = null;

			try
			{
				response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

				response.EnsureSuccessStatusCode();

				Stream upcomingJsonStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

				upcomingData = await JsonSerializer.DeserializeAsync<UpcomingData>(upcomingJsonStream, jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
			}
			catch (HttpRequestException) { }
			catch (IOException) { }
			catch (SocketException) { }
			catch (TimeoutException) { }
			catch (OperationCanceledException) { }
			finally
			{
				client.Dispose();
				handler.Dispose();
				request.Dispose();
				response?.Dispose();
			}

			return upcomingData;
		}
	}
}
