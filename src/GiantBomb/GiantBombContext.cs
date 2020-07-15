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

        private readonly ILog logger;
        private readonly ISettings settings;

        public GiantBombContext(ILog logger, ISettings settings)
        {
            this.logger = logger;
            this.settings = settings;

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
            HttpStatusCode status;
            UpcomingData? upcomingData;

            try
            {
                (status, upcomingData) = await DownloadAsync(settings.Upcoming).ConfigureAwait(false);
            }
            catch (HttpRequestException)
            {
                return new Response { Reason = Reason.InternetError };
            }
            catch (OperationCanceledException)
            {
                return new Response { Reason = Reason.Timeout };
            }
            
            if (status != HttpStatusCode.OK)
            {
                return new Response { Reason = Reason.NotOk };
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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Version = HttpVersion.Version20
            };

            request.Headers.Accept.ParseAdd("application/json; charset=utf-8");
            request.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
            request.Headers.Connection.ParseAdd("close"); // Connection is not used under HTTP/2, but the worst they can do is ignore it
            request.Headers.Host = "www.giantbomb.com";
                        
            if (!String.IsNullOrWhiteSpace(settings.UserAgent))
            {
                request.Headers.UserAgent.ParseAdd(settings.UserAgent);
            }
            
            HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            Stream? stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            UpcomingData upcomingResponse = await JsonSerializer.DeserializeAsync<UpcomingData>(stream, serializerOptions).ConfigureAwait(false);
            
            request.Dispose();
            stream?.Dispose();
            response.Dispose();

            return (response.StatusCode, upcomingResponse);
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

            logger.Message($"converted {data.Count} data entries into {shows.Count} shows", Severity.Debug);

            return shows.AsReadOnly();
        }

        private Uri GetImageUri(string raw)
        {
            if (raw.StartsWith("https://"))
            {
                return Uri.TryCreate(raw, UriKind.Absolute, out Uri? uri) ? uri : settings.FallbackImage;
            }
            else
            {
                return Uri.TryCreate($"https://{raw}", UriKind.Absolute, out Uri? uri) ? uri : settings.FallbackImage;
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
