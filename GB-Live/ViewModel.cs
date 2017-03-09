using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using GB_Live.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GB_Live
{
    public class ViewModel : ViewModelBase
    {
        #region Commands
        private DelegateCommandAsync _refreshCommandAsync = null;
        public DelegateCommandAsync RefreshCommandAsync
        {
            get
            {
                if (_refreshCommandAsync == null)
                {
                    _refreshCommandAsync = new DelegateCommandAsync(RefreshAsync, canExecuteAsync);
                }

                return _refreshCommandAsync;
            }
        }

        private async Task RefreshAsync()
        {
            IsBusy = true;

            await UpdateAsync();

            IsBusy = false;
        }

        private DelegateCommand _goToHomepageInBrowserCommand = null;
        public DelegateCommand GoToHomepageInBrowserCommand
        {
            get
            {
                if (_goToHomepageInBrowserCommand == null)
                {
                    _goToHomepageInBrowserCommand = new DelegateCommand(GoToHomepageInBrowser, canExecute);
                }

                return _goToHomepageInBrowserCommand;
            }
        }

        private static void GoToHomepageInBrowser()
        {
            Uri uri = new Uri(ConfigurationManager.AppSettings["GBHomepage"]);

            Utils.OpenUriInBrowser(uri);
        }

        private DelegateCommand _goToChatPageInBrowserCommand = null;
        public DelegateCommand GoToChatPageInBrowserCommand
        {
            get
            {
                if (_goToChatPageInBrowserCommand == null)
                {
                    _goToChatPageInBrowserCommand = new DelegateCommand(GoToChatPageInBrowser, canExecute);
                }

                return _goToChatPageInBrowserCommand;
            }
        }

        private static void GoToChatPageInBrowser()
        {
            Uri uri = new Uri(ConfigurationManager.AppSettings["GBChat"]);

            Utils.OpenUriInBrowser(uri);
        }
        
        private bool canExecute(object _)
        {
            return true;
        }

        private bool canExecuteAsync(object _)
        {
            return !IsBusy;
        }
        #endregion

        #region Fields
        private const string appName = "GB Live";

        private readonly DispatcherTimer updateTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
        {
            Interval = TimeSpan.FromMinutes(3)
        };
        #endregion

        #region Properties
        public string WindowTitle
        {
            get
            {
                return IsBusy ? string.Format(CultureInfo.CurrentCulture, "{0}: checking ...", appName) : appName;
            }
        }

        private bool _isLive = false;
        public bool IsLive
        {
            get
            {
                return _isLive;
            }
            set
            {
                if (_isLive != value)
                {
                    _isLive = value;

                    RaisePropertyChanged(nameof(IsLive));
                    RaisePropertyChanged(nameof(LiveShowName));
                }
            }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;

                    RaisePropertyChanged(nameof(IsBusy));
                    RaisePropertyChanged(nameof(WindowTitle));

                    RaiseAllCommandCanExecuteChanged();
                }
            }
        }

        private string _liveShowName = string.Empty;
        public string LiveShowName
        {
            get
            {
                return IsLive ? _liveShowName : "There is no live show";
            }
            set
            {
                _liveShowName = value;

                RaisePropertyChanged(nameof(LiveShowName));
            }
        }

        private void RaiseAllCommandCanExecuteChanged()
        {
            RefreshCommandAsync.RaiseCanExecuteChanged();
        }

        private readonly ObservableCollection<GBUpcomingEvent> _events = new ObservableCollection<GBUpcomingEvent>();
        public IReadOnlyCollection<GBUpcomingEvent> Events { get { return _events; } }
        #endregion

        public ViewModel()
        {
            _events.CollectionChanged += Events_CollectionChanged;

            updateTimer.Tick += async (s, e) => await UpdateAsync();
            updateTimer.Start();
        }
        
        private static void Events_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            /*
             * 
             * DO NOT USE EVENTS.CLEAR() !!!!!!!!!!
             * 
             * Events.Clear() fires NotifyCollectionChangedEventArgs.Reset - NOT .Remove
             * 
             * .Reset does not give you a list of what was removed
             * 
             * we need access to each removed event in order to unhook the countdowntimer event
             *
             */

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    StartEventTimers(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    StopEventTimers(e.OldItems);
                    break;
                default:
                    break;
            }
        }

        private static void StartEventTimers(IList newItems)
        {
            foreach (GBUpcomingEvent each in newItems)
            {
                each?.StartCountdownTimer();
            }
        }

        private static void StopEventTimers(IList oldItems)
        {
            foreach (GBUpcomingEvent each in oldItems)
            {
                each?.StopCountdownTimer();
            }
        }

        public async Task UpdateAsync()
        {
            string web = await DownloadUpcomingJsonAsync();

            if (String.IsNullOrEmpty(web)) { return; }

            JObject json = ParseIntoJson(web);

            if (json == null) { return; }

            NotifyIfLive(json);

            ProcessEvents(json);
        }
        
        private async static Task<string> DownloadUpcomingJsonAsync()
        {
            Uri jsonUri = new Uri(ConfigurationManager.AppSettings["GBUpcomingJson"]);

            HttpWebRequest req = BuildRequest(jsonUri);

            return await Utils.DownloadWebsiteAsStringAsync(req).ConfigureAwait(false);
        }

        private static JObject ParseIntoJson(string web)
        {
            JObject json = null;

            try
            {
                json = JObject.Parse(web);
            }
            catch (JsonReaderException ex)
            {
                Utils.LogException(ex);
            }

            return json;
        }

        private void NotifyIfLive(JObject json)
        {
            if (json["liveNow"].HasValues)
            {
                LiveShowName = GetLiveShowName(json["liveNow"]);

                if (IsLive == false)
                {
                    IsLive = true;

                    NameValueCollection nvc = ConfigurationManager.AppSettings;

                    string liveMessage = nvc["GBIsLiveMessage"];
                    Uri chatUri = new Uri(nvc["GBChat"]);

                    NotificationService.Send(liveMessage, () => Utils.OpenUriInBrowser(chatUri));
                }
            }
            else
            {
                IsLive = false;
            }
        }

        private static string GetLiveShowName(JToken token)
        {
            return (string)token["title"] ?? "Untitled Live Show";
        }

        private void ProcessEvents(JObject json)
        {
            IReadOnlyList<GBUpcomingEvent> eventsFromWeb = GetEvents(json);

            foreach (GBUpcomingEvent each in eventsFromWeb)
            {
                // we don't want to add any events that are already in the collection, hence
                //      -> !_events.Contains(each)
                //
                // or that are for a time in the past, hence
                //      -> each.Time > DateTime.Now
                //
                // sometimes the JSON will contain old events after the expired time

                if (!_events.Contains(each) && (each.Time > DateTime.Now))
                {
                    _events.Add(each);
                }
            }

            RemoveOld(eventsFromWeb);
        }

        private static IReadOnlyList<GBUpcomingEvent> GetEvents(JObject json)
        {
            List<GBUpcomingEvent> events = new List<GBUpcomingEvent>();
            
            if (json.TryGetValue("upcoming", StringComparison.Ordinal, out JToken upcoming))
            {
                foreach (JToken each in upcoming)
                {
                    if (GBUpcomingEvent.TryCreate(each, out GBUpcomingEvent newEvent))
                    {
                        events.Add(newEvent);
                    }
                }
            }
            
            return events;
        }
        
        private void RemoveOld(IEnumerable<GBUpcomingEvent> eventsFromHtml)
        {
            /*
             * 
             * DO NOT USE EVENTS.CLEAR() !!!!!!!!!!
             * 
             * Events.Clear() fires NotifyCollectionChangedEventArgs.Reset - NOT .Remove
             * 
             * .Reset does not give you a list of what was removed
             * 
             * we need access to each removed event in order to unhook the countdowntimer event
             *
             */

            // We want to remove the following types of events:
            // DateTime.MaxValue: the countdown has fired
            // x.Time < DateTime.Now: the event is in the past
            // !eventsFromHtml.Contains(x): event removed before time (e.g. cancelled, rescheduled)

            var toRemove = _events
                .Where(x => x.Time == DateTime.MaxValue || x.Time < DateTime.Now || !eventsFromHtml.Contains(x))
                .ToList();

            _events.RemoveRange(toRemove);
        }

        private static HttpWebRequest BuildRequest(Uri gbUri)
        {
            HttpWebRequest req = WebRequest.CreateHttp(gbUri);

            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            req.Host = gbUri.DnsSafeHost;
            req.KeepAlive = false;
            req.Method = "GET";
            req.ProtocolVersion = HttpVersion.Version11;
            req.Timeout = 2500;
            req.UserAgent = ConfigurationManager.AppSettings["UserAgent"];
            
            req.Headers.Add("DNT", "1");
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            
            return req;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().Name);
            sb.AppendLine(WindowTitle);
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "IsLive: {0}", IsLive));
            sb.AppendLine(LiveShowName);
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Number of events: {0}", Events.Count));

            return sb.ToString();
        }

#if DEBUG
        public void DEBUG_set_live()
        {
            IsLive = !IsLive;
        }

        public void DEBUG_add(GBUpcomingEvent item)
        {
            _events.Add(item);
        }
#endif
    }
}