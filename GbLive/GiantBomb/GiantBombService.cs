using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GbLive.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GbLive.GiantBomb
{
    public static class GiantBombService
    {
        private static HttpClient client = null;

        private static void InitClient()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                MaxAutomaticRedirections = 2
            };

            client = new HttpClient(handler, disposeHandler: true);

            client.DefaultRequestHeaders.Add("User-Agent", Settings.UserAgent);
        }
        
        public static async Task<UpcomingResponse> UpdateAsync()
        {
            if (client == null)
            {
                InitClient();
            }
            
            (string raw, HttpStatusCode status) = await DownloadUpcomingAsync().ConfigureAwait(false);
            
            if (status != HttpStatusCode.OK)
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "downloading JSON failed, status: {0}", status);

                await Log.LogMessageAsync(errorMessage).ConfigureAwait(false);

                return null;
            }
            
            return Parse(raw) is JObject json
                ? Process(json)
                : null;
        }
        
        private static async Task<(string raw, HttpStatusCode status)> DownloadUpcomingAsync()
        {
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(Settings.Upcoming).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return (string.Empty, response.StatusCode);
                    }
                    
                    string text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return (text, response.StatusCode);
                }
            }
            catch (HttpRequestException) { }
            catch (TaskCanceledException ex) when (ex.InnerException is HttpRequestException) { }
            catch (TaskCanceledException ex)
            {
                await Log.LogExceptionAsync(ex).ConfigureAwait(false);
            }

            return (string.Empty, default(HttpStatusCode));
        }

        private static JObject Parse(string raw)
        {
            JObject json = null;

            try
            {
                json = JObject.Parse(raw);
            }
            catch (JsonReaderException ex)
            {
                Log.LogException(ex);
            }

            return json;
        }

        private static UpcomingResponse Process(JObject json)
        {
            (bool isLive, string liveShowName) = GetLiveDetails(json);
            IEnumerable<UpcomingEvent> events = GetEvents(json);

            return new UpcomingResponse(isLive, liveShowName, events);
        }
        
        private static (bool isLive, string liveShowName) GetLiveDetails(JObject json)
        {
            bool isLive = false;
            string liveShowName = Settings.NameOfNoLiveShow;

            if (!json.TryGetValue("liveNow", StringComparison.OrdinalIgnoreCase, out JToken liveNow))
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "tag 'liveNow' not found in JObject");

                Log.LogMessage(errorMessage);

                return (isLive, liveShowName);
            }

            isLive = liveNow.HasValues;

            if (isLive)
            {
                liveShowName = (string)liveNow["title"] ?? Settings.NameOfUntitledLiveShow;
            }

            return (isLive, liveShowName);
        }

        private static IEnumerable<UpcomingEvent> GetEvents(JObject json)
        {
            if (!json.TryGetValue("upcoming", StringComparison.OrdinalIgnoreCase, out JToken upcoming))
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "tag 'upcoming' not found");

                Log.LogMessage(errorMessage);

                return Enumerable.Empty<UpcomingEvent>();
            }

            var events = new List<UpcomingEvent>();

            foreach (JToken each in upcoming)
            {
                if (UpcomingEvent.TryCreate(each, out UpcomingEvent ue))
                {
                    events.Add(ue);
                }
            }

            return events;
        }
    }
}
