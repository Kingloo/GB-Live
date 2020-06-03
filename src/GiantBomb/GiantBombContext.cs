using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;
using GBLive.Common;
using GBLive.GiantBomb.Interfaces;

namespace GBLive.GiantBomb
{
    public class GiantBombContext : IGiantBombContext, IDisposable
    {
        private readonly SocketsHttpHandler handler;
        private readonly HttpClient client;

        private readonly ILog _logger;
        private readonly ISettings _settings;

        public GiantBombContext(ILog logger, ISettings settings)
        {
            _logger = logger;
            _settings = settings;

            handler = new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
                MaxConnectionsPerServer = 5,
                SslOptions = new SslClientAuthenticationOptions
                {
                    AllowRenegotiation = false,
                    ApplicationProtocols = new List<SslApplicationProtocol> { SslApplicationProtocol.Http2 },
                    // GiantBomb does not yet support Tls13, remove Tls12 when they eventually do
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    EncryptionPolicy = EncryptionPolicy.RequireEncryption
                }
            };

            client = new HttpClient(handler, false)
            {
                Timeout = TimeSpan.FromSeconds(30d)
            };

            if (!client.DefaultRequestHeaders.UserAgent.TryParseAdd(_settings.UserAgent))
            {
                string message = $"could not add UserAgent ({_settings.UserAgent})";

                _logger.Message(message, Severity.Error);

                throw new ArgumentException(message, _settings.UserAgent);
            }
        }

        public async Task<IResponse> UpdateAsync()
        {
            if (_settings.Upcoming is null)
            {
                return new Response { Reason = Reason.MissingUri };
            }

            (HttpStatusCode status, ReadOnlySpan<byte> raw) = await DownloadStringAsync(_settings.Upcoming).ConfigureAwait(false);

            if (status != HttpStatusCode.OK)
            {
                return new Response { Reason = Reason.InternetError };
            }
            
            if (raw.IsEmpty)
            {
                return new Response { Reason = Reason.Empty };
            }
            
            if (!TryParse(raw, out JsonElement? r))
            {
                return new Response { Reason = Reason.ParseFailed };
            }

#nullable disable
            JsonElement root = (JsonElement)r;
#nullable enable

            return ProcessJson(root);
        }

        private async Task<(HttpStatusCode, byte[])> DownloadStringAsync(Uri uri)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            byte[] bytes = Array.Empty<byte>();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Version = HttpVersion.Version20
            };

            HttpResponseMessage? response = null;

            try
            {
                response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException) { }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is Win32Exception inner)
                {
                    await _logger.ExceptionAsync(ex, inner.Message).ConfigureAwait(false);
                }
            }
            finally
            {
                request.Dispose();

                if (response != null)
                {
                    status = response.StatusCode;

                    response.Dispose();
                }
            }

            return (status, bytes);
        }

        private bool TryParse(ReadOnlySpan<byte> raw, out JsonElement? root)
        {
            Utf8JsonReader reader = new Utf8JsonReader(raw);

            if (JsonDocument.TryParseValue(ref reader, out JsonDocument document))
            {
                root = document.RootElement;
                return true;
            }
            else
            {
                root = null;
                return false;
            }
        }

        private IResponse ProcessJson(JsonElement element)
        {
            IResponse response = new Response
            {
                Reason = Reason.Success
            };

            response.IsLive = GetIsLive(element);

            if (response.IsLive)
            {
                string title = GetLiveShowTitle(element);

                if (!String.IsNullOrWhiteSpace(title))
                {
                    response.LiveShowTitle = title;
                }
            }

            response.Shows = GetShows(element);

            return response;
        }

        private static bool GetIsLive(JsonElement root)
        {
            return root.TryGetProperty("liveNow", out JsonElement liveNow)
                && liveNow.ValueKind == JsonValueKind.Object;
        }

        private static string GetLiveShowTitle(JsonElement root)
        {
            string title = string.Empty;

            if (root.TryGetProperty("liveNow", out JsonElement liveNow))
            {
                if (liveNow.TryGetProperty("title", out JsonElement titleElement))
                {
                    title = titleElement.GetString();
                }
            }

            return title;
        }

        private IReadOnlyCollection<IShow> GetShows(JsonElement root)
        {
            Collection<IShow> shows = new Collection<IShow>();
            
            if (root.TryGetProperty("upcoming", out JsonElement upcoming))
            {
                if (upcoming.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement each in upcoming.EnumerateArray())
                    {
                        IShow show = CreateShow(each);

                        if (!shows.Contains(show))
                        {
                            shows.Add(show);
                        }
                    }
                }
            }

            return shows;
        }

        private IShow CreateShow(JsonElement element)
        {
            IShow show = new Show();

            if (element.TryGetProperty("title", out JsonElement title))
            {
                show.Title = title.GetString();
            }

            if (element.TryGetProperty("date", out JsonElement date))
            {
                show.Time = GetDate(date.GetString());
            }

            if (element.TryGetProperty("premium", out JsonElement isPremium))
            {
                show.IsPremium = isPremium.GetBoolean();
            }

            if (element.TryGetProperty("type", out JsonElement type))
            {
                show.ShowType = type.GetString();
            }

            if (element.TryGetProperty("image", out JsonElement image))
            {
                show.Image = Uri.TryCreate($"https://{image.GetString()}", UriKind.Absolute, out Uri? uri) ? uri : _settings.FallbackImage;
            }

            return show;
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
        private bool disposedValue = false; // To detect redundant calls

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
