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

namespace GBLive.GiantBomb
{
	public static class Updater
	{
		private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};

		public static ValueTask<UpcomingData?> UpdateAsync(Uri upcomingJsonUri)
			=> UpdateAsync(upcomingJsonUri, CancellationToken.None);

		public static async ValueTask<UpcomingData?> UpdateAsync(Uri upcomingJsonUri, CancellationToken cancellationToken)
		{
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
					EncryptionPolicy = System.Net.Security.EncryptionPolicy.RequireEncryption
				}
			};

			HttpClient client = new HttpClient(handler, disposeHandler: false);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, upcomingJsonUri)
			{
				Version = HttpVersion.Version20
			};

			request.Headers.Accept.ParseAdd("application/json; charset=utf-8");
			request.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
			request.Headers.Connection.ParseAdd("close"); // Connection is not used under HTTP/2, but the worst they can do is ignore it
			request.Headers.Host = "www.giantbomb.com"; // this MUST have www - no idea why
			request.Headers.UserAgent.ParseAdd(Constants.UserAgent);

			HttpResponseMessage? response = null;

			try
			{
				response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

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
