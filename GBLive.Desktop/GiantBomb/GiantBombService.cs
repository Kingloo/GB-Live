using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GBLive.Desktop.Common;
using GBLive.Desktop.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GBLive.Desktop.GiantBomb
{
    public class GiantBombService : IGiantBombService, IDisposable
    {
        #region Fields
        private readonly HttpClient client = new HttpClient();
        private string jsonResponseCache = string.Empty;
        #endregion

        #region Properties
        private bool _isLive = false;
        public bool IsLive => _isLive;

        private string _liveShowName = string.Empty;
        public string LiveShowName => _liveShowName;

        private readonly ObservableCollection<UpcomingEvent> _events
            = new ObservableCollection<UpcomingEvent>();
        public IReadOnlyCollection<UpcomingEvent> Events => _events;
        #endregion

        public GiantBombService()
        {
            client.DefaultRequestHeaders.Add("User-Agent", Settings.UserAgent);
        }
        
        public void AddUpcomingEvent(UpcomingEvent upcomingEvent)
        {
            if (upcomingEvent == null) { throw new ArgumentNullException(nameof(upcomingEvent)); }

            if (!_events.Contains(upcomingEvent) && upcomingEvent.Time > DateTime.Now)
            {
                _events.Add(upcomingEvent);

                upcomingEvent.StartCountdownTimer();
            }
        }

        public void RemoveUpcomingEvent(UpcomingEvent upcomingEvent)
        {
            if (upcomingEvent == null) { throw new ArgumentNullException(nameof(upcomingEvent)); }

            if (_events.Contains(upcomingEvent))
            {
                upcomingEvent.StopCountdownTimer();

                _events.Remove(upcomingEvent);
            }
        }

        public async Task UpdateAsync()
        {
            (string raw, HttpStatusCode status) = await DownloadJsonAsync();

            if (status != HttpStatusCode.OK)
            {
                string error = string.Format(
                    CultureInfo.CurrentCulture,
                    "request for {0} failed: {1}",
                    Settings.UpcomingJson.AbsoluteUri,
                    status.ToString());

                await Log.LogMessageAsync(error).ConfigureAwait(false);

                return;
            }

            if (raw.Equals(jsonResponseCache))
            {
                return;
            }
            else
            {
                jsonResponseCache = raw;
            }
            
            if (Parse(raw) is JObject json)
            {
                Process(json);
            }
        }
        
        private async Task<(string raw, HttpStatusCode status)> DownloadJsonAsync()
        {
            try
            {
                using (var response = await client.GetAsync(Settings.UpcomingJson).ConfigureAwait(false))
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
            if (json.TryGetValue("liveNow", StringComparison.OrdinalIgnoreCase, out JToken liveNow))
            {
                SetLiveStatus(liveNow);

                SetLiveShowName(liveNow);
            }

            if (json.TryGetValue("upcoming", StringComparison.OrdinalIgnoreCase, out JToken upcoming))
            {
                var events = CreateEvents(upcoming);
                
                foreach (var each in events)
                {
                    AddUpcomingEvent(each);
                }
                
                RemoveExpired(events);
            }
        }
        
        private void SetLiveStatus(JToken token) => _isLive = token.HasValues;

        private void SetLiveShowName(JToken token)
        {
            if (token.HasValues)
            {
                _liveShowName = (string)token["title"] ?? "Untitled Live Show";
            }
            else
            {
                _liveShowName = "No live show";
            }
        }

        private static IEnumerable<UpcomingEvent> CreateEvents(JToken upcoming)
        {
            var events = new List<UpcomingEvent>();
            
            foreach (JToken each in upcoming)
            {
                if (UpcomingEvent.TryCreate(each, out UpcomingEvent newEvent))
                {
                    events.Add(newEvent);
                }
            }

            return events.Any() ? events : Enumerable.Empty<UpcomingEvent>();
        }
        
        private void RemoveExpired(IEnumerable<UpcomingEvent> eventsFromWeb)
        {
            /*
             Events are to be removed when
               a) Time is set to MaxValue, see FormatDateTimeString
               b) Time is in the past, aka < DateTime.Now
               c) the event is not in the JSON returned from the site
             */

            var toRemove = from each in _events
                           where each.Time == DateTime.MaxValue
                           || each.Time < DateTime.Now
                           || !eventsFromWeb.Contains(each)
                           select each;

            // NEVER use .Reset
            // .Reset does not provide a list of what was removed
            // we need such a list so as to be able to unhook the countdown timer events

            _events.RemoveRange(toRemove.ToList());
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "{0} events", Events.Count));

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
