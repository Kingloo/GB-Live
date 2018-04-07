using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GbLive.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GbLive.GiantBomb
{
    public static class GiantBombService
    {
        private static HttpClient client = default;

        private static void InitClient()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
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
                return new UpcomingResponse(false, $"downloading JSON failed ({status.ToString()})");
            }

            if (Parse(raw) is JObject json)
            {
                return Process(json);
            }
            else
            {
                return new UpcomingResponse(false, "response did not parse into JSON");
            }
        }
        
        private static async Task<(string raw, HttpStatusCode status)> DownloadUpcomingAsync()
        {
            string text = string.Empty;
            HttpStatusCode status = HttpStatusCode.Unused;

            HttpResponseMessage response = default;

            try
            {
                using (response = await client.GetAsync(Settings.Upcoming, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (HttpRequestException) { }
            catch (TaskCanceledException ex) when (ex.InnerException is HttpRequestException) { }
            catch (TaskCanceledException ex)
            {
                if (ex.InnerException is Exception inner)
                {
                    await Log.LogExceptionAsync(inner).ConfigureAwait(false);
                }
            }
            finally
            {
                if (response != null)
                {
                    status = response.StatusCode;

                    response.Dispose();
                }
            }

            return (text, status);
        }

        private static JObject Parse(string raw)
        {
            JObject json = null;

            try
            {
                json = JObject.Parse(raw);
            }
            catch (JsonReaderException) { }

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
                string errorMessage = "tag 'liveNow' not found in JObject";

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
                string errorMessage = "tag 'upcoming' not found";

                Log.LogMessage(errorMessage);

                yield break;
            }

            foreach (JToken each in upcoming)
            {
                if (UpcomingEvent.TryCreate(each, out UpcomingEvent ue))
                {
                    yield return ue;
                }
            }
        }
    }
}
