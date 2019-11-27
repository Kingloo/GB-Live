using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading.Tasks;
using GBLive.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GBLive.GiantBomb
{
    public static class GiantBombService
    {
        private static readonly SocketsHttpHandler handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
            MaxConnectionsPerServer = 5,
            SslOptions = new SslClientAuthenticationOptions
            {
                AllowRenegotiation = false,
                ApplicationProtocols = new List<SslApplicationProtocol> { SslApplicationProtocol.Http2 },
                EnabledSslProtocols = SslProtocols.Tls13,
                EncryptionPolicy = EncryptionPolicy.RequireEncryption
            }
        };

        private static readonly HttpClient client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(5d)
        };

        public static async Task<UpcomingResponse> UpdateAsync()
        {
            SetUserAgentHeader();

            (HttpStatusCode status, string text) = await Web.DownloadStringAsync(Settings.Upcoming).ConfigureAwait(false);

            if (status != HttpStatusCode.OK)
            {
                return new UpcomingResponse
                {
                    Reason = Reason.InternetError
                };
            }

            if (String.IsNullOrWhiteSpace(text))
            {
                return new UpcomingResponse
                {
                    Reason = Reason.TextEmpty
                };
            }

            if (TryParse(text, out UpcomingResponse? response) && response != null)
            {
                return response;
            }
            else
            {
                return new UpcomingResponse { Reason = Reason.ParseFailed };
            }
        }

        private static void SetUserAgentHeader()
        {
            if (client.DefaultRequestHeaders.UserAgent.Count > 1)
            {
                client.DefaultRequestHeaders.UserAgent.Clear();
            }

            if (client.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                if (!client.DefaultRequestHeaders.UserAgent.TryParseAdd(Settings.UserAgent))
                {
                    throw new ArgumentException($"couldn't add user agent to GiantBombService: {Settings.UserAgent}", nameof(Settings.UserAgent));
                }
            }
        }

        private static bool TryParse(string text, out UpcomingResponse? response)
        {
            JObject json;

            try
            {
                json = JObject.Parse(text);
            }
            catch (JsonReaderException)
            {
                response = null;
                return false;
            }

            bool isLive = json.TryGetValue("liveNow", StringComparison.Ordinal, out JToken liveNow)
                ? liveNow.HasValues
                : false;

            string liveShowTitle = (liveNow != null && liveNow.HasValues)
                ? (string)liveNow["title"]
                : Settings.NameOfUntitledLiveShow;

            response = new UpcomingResponse
            {
                Reason = Reason.Success,
                IsLive = isLive,
                LiveShowTitle = liveShowTitle,
                Events = GetEvents(json)
            };

            return true;
        }

        private static IEnumerable<UpcomingEvent> GetEvents(JObject json)
        {
            var sco = StringComparison.Ordinal;

            if (!json.TryGetValue("upcoming", sco, out JToken upcoming))
            {
                return Enumerable.Empty<UpcomingEvent>();
            }
            
            if (upcoming.Type != JTokenType.Array)
            {
                return Enumerable.Empty<UpcomingEvent>();
            }

            var events = new Collection<UpcomingEvent>();

            foreach (JObject each in upcoming)
            {
                string title = each.TryGetValue("title", sco, out JToken titleToken)
                    ? (string)titleToken
                    : "Untitled";

                DateTimeOffset date = each.TryGetValue("date", sco, out JToken dateToken)
                    ? GetDate((string)dateToken)
                    : DateTimeOffset.MaxValue;
                
                bool isPremium = each.TryGetValue("premium", sco, out JToken isPremiumToken)
                    ? (bool)isPremiumToken
                    : false;
                
                string type = each.TryGetValue("type", sco, out JToken typeToken)
                    ? (string)typeToken
                    : "";

                string uri = each.TryGetValue("image", sco, out JToken imageToken)
                    ? (string)imageToken
                    : string.Empty;
                
                Uri image = Uri.TryCreate($"https://{uri}", UriKind.Absolute, out Uri? u) ? u : Settings.FallbackImage;

                UpcomingEvent ue = new UpcomingEvent
                {
                    Title = title,
                    Time = date,
                    IsPremium = isPremium,
                    EventType = type,
                    Image = image
                };

                events.Add(ue);
            }

            return events;
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
    }
}