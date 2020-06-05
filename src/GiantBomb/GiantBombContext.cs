using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;
using GBLive.Common;
using GBLive.GiantBomb.Interfaces;

namespace GBLive.GiantBomb
{
    public class GiantBombContext : IGiantBombContext, IDisposable
    {
        private readonly HttpClientHandler handler;
        private readonly HttpClient client;

        private readonly ILog _logger;
        private readonly ISettings _settings;

        public GiantBombContext(ILog logger, ISettings settings)
        {
            _logger = logger;
            _settings = settings;

            handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
                MaxAutomaticRedirections = 3,
                MaxConnectionsPerServer = 1,
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
            };

            client = new HttpClient(handler, false)
            {
                Timeout = TimeSpan.FromSeconds(10d)
            };
        }

        public async Task<IResponse> UpdateAsync()
        {
            (HttpStatusCode status, UpcomingData? upcomingData) = await DownloadAsync(_settings.Upcoming).ConfigureAwait(false);

            if (status != HttpStatusCode.OK)
            {
                return new Response { Reason = Reason.InternetError };
            }

            if (upcomingData is null)
            {
                return new Response { Reason = Reason.Empty };
            }

            Response response = new Response
            {
                Reason = Reason.Success,
                LiveNow = upcomingData.LiveNow,
                Shows = ConvertData(upcomingData.Upcoming)
            };

            return response;
        }

        private async Task<(HttpStatusCode, UpcomingData?)> DownloadAsync(Uri uri)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            UpcomingData? upcomingResponse = null;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Version = HttpVersion.Version20
            };

            request.Headers.Accept.ParseAdd("application/json; charset=utf-8");
            request.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            request.Headers.Connection.ParseAdd("close"); // Connection is not used under HTTP/2, but the worst they can do is ignore it
            request.Headers.Host = "www.giantbomb.com";
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0");

            request.Headers.Add("DNT", "1");

            HttpResponseMessage? response = null;
            Stream? stream = null;

            try
            {
                response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                upcomingResponse = await JsonSerializer.DeserializeAsync<UpcomingData>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is Win32Exception inner)
                {
                    await _logger.ExceptionAsync(ex, inner.Message).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException ex)
            {
                await _logger.ExceptionAsync(ex, "probably timed out").ConfigureAwait(false);
            }
            finally
            {
                request.Dispose();

                stream?.Dispose();

                if (response != null)
                {
                    status = response.StatusCode;

                    response.Dispose();
                }
            }

            return (status, upcomingResponse);
        }

        private IReadOnlyCollection<Show> ConvertData(IReadOnlyCollection<ShowData> data)
        {
            List<Show> shows = new List<Show>();

            foreach (ShowData each in data)
            {
                Show show = new Show
                {
                    ShowType = each.Type,
                    Title = each.Title,
                    Image = GetImageUri(each.Image),
                    IsPremium = each.Premium,
                    Time = GetDate(each.Date)
                };

                shows.Add(show);
            }

            return shows.AsReadOnly();
        }

        private Uri GetImageUri(string raw)
        {
            if (raw.StartsWith("https://"))
            {
                return Uri.TryCreate(raw, UriKind.Absolute, out Uri? uri) ? uri : _settings.FallbackImage;
            }
            else
            {
                return Uri.TryCreate($"https://{raw}", UriKind.Absolute, out Uri? uri) ? uri : _settings.FallbackImage;
            }
        }

        private static DateTimeOffset GetDate(string date)
        {
            if (!DateTimeOffset.TryParse(date, out DateTimeOffset dt))
            {
                return DateTimeOffset.MaxValue;
            }

            TimeSpan pacific = TimeZoneInfo
                .GetSystemTimeZones()
                .Single(tz => tz.Id == "Pacific Standard Time")
                .GetUtcOffset(DateTimeOffset.Now);

            TimeSpan local = TimeZoneInfo.Local.GetUtcOffset(DateTimeOffset.Now);

            TimeSpan offset = local - pacific;

            return dt.Add(offset);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client.Dispose();
                    handler.Dispose();
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
