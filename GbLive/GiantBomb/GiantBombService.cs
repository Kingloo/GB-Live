using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class GiantBombService : IDisposable
    {
        private static HttpClient client = null;

        #region Properties
        private bool _isLive = false;
        public bool IsLive => _isLive;

        private string _liveShowName = string.Empty;
        public string LiveShowName => _liveShowName;

        private readonly List<UpcomingEvent> _events = new List<UpcomingEvent>();
        public IReadOnlyList<UpcomingEvent> Events => _events;
        #endregion

        public GiantBombService()
        {
            if (client == null)
            {
                var handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    MaxAutomaticRedirections = 3
                };

                client = new HttpClient(handler, disposeHandler: true);

                client.DefaultRequestHeaders.Add("User-Agent", Settings.UserAgent);
            }
        }

        public async Task UpdateAsync()
        {
            (string raw, HttpStatusCode status) = await DownloadUpcomingAsync().ConfigureAwait(false);

            if (status != HttpStatusCode.OK)
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "downloading JSON failed, status: {0}", status);

                await Log.LogMessageAsync(errorMessage).ConfigureAwait(false);

                return;
            }

            if (Parse(raw) is JObject json)
            {
                Process(json);
            }
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

        private void Process(JObject json)
        {
            SetLiveDetails(json);
            CreateEvents(json);
        }
        
        private void SetLiveDetails(JObject json)
        {
            string tag = "liveNow";

            if (json.TryGetValue(tag, StringComparison.OrdinalIgnoreCase, out JToken liveNow))
            {
                _isLive = liveNow.HasValues;

                if (_isLive)
                {
                    _liveShowName = (string)liveNow["title"] ?? Settings.NameOfUntitledLiveShow;
                }
                else
                {
                    _liveShowName = Settings.NameOfNoLiveShow;
                }
            }
            else
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "{0} not found in JObject", tag);

                Log.LogMessage(errorMessage);
            }
        }

        private void CreateEvents(JObject json)
        {
            string tag = "upcoming";

            if (json.TryGetValue(tag, StringComparison.OrdinalIgnoreCase, out JToken upcoming))
            {
                foreach (JToken each in upcoming)
                {
                    CreateAndAdd(each);
                }
            }
            else
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "{0} not found in JObject", tag);

                Log.LogMessage(errorMessage);
            }
        }

        private void CreateAndAdd(JToken each)
        {
            if (UpcomingEvent.TryCreate(each, out UpcomingEvent newEvent))
            {
                if (!_events.Contains(newEvent) && newEvent.Time > DateTime.Now)
                {
                    _events.Add(newEvent);
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Upcoming.TryCreate failed: ");
                sb.AppendLine(each.ToString());

                Log.LogMessage(sb.ToString());
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Is Live: {0}", IsLive.ToString(CultureInfo.CurrentCulture)));
            sb.AppendLine(LiveShowName);

            foreach (UpcomingEvent each in Events)
            {
                sb.AppendLine(each.ToString());
            }

            return sb.ToString();
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
