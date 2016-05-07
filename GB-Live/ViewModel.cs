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
using System.Windows;
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
            await UpdateAsync();
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

        private DelegateCommand _exitCommand = null;
        public DelegateCommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new DelegateCommand(Exit, canExecute);
                }

                return _exitCommand;
            }
        }

        private static void Exit()
        {
            Application.Current.MainWindow.Close();
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
                if (!value == _isLive)
                {
                    _isLive = value;

                    OnNotifyPropertyChanged();
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
                if (!value == _isBusy)
                {
                    _isBusy = value;

                    OnNotifyPropertyChanged();
                    OnNotifyPropertyChanged("WindowTitle");

                    RaiseAllCommandCanExecuteChanged();
                }
            }
        }

        private void RaiseAllCommandCanExecuteChanged()
        {
            RefreshCommandAsync.RaiseCanExecuteChanged();
        }

        private readonly ObservableCollection<GBUpcomingEvent> _events = new ObservableCollection<GBUpcomingEvent>();
        public ObservableCollection<GBUpcomingEvent> Events { get { return _events; } }
        #endregion

        public ViewModel()
        {
            Events.CollectionChanged += Events_CollectionChanged;

            updateTimer.Tick += async (s, e) => await UpdateAsync();
            updateTimer.Start();
        }
        
        private void Events_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            /*
             * 
             * DO NOT USE EVENTS.CLEAR !!!!!!!!!!!
             * 
             * Events.Clear() fires NotifyCollectionChangedEventsArgs.Reset - NOT .Remove
             * 
             * .Reset does not give you a list of what was removed
             *
             */

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    StartAllEventTimers(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    StopAllEventTimers(e.OldItems);
                    break;
                default:
                    break;
            }
        }

        private static void StartAllEventTimers(IList newItems)
        {
            foreach (GBUpcomingEvent each in newItems)
            {
                each?.StartCountdownTimer();
            }
        }

        private static void StopAllEventTimers(IList oldItems)
        {
            foreach (GBUpcomingEvent each in oldItems)
            {
                each?.StopCountdownTimer();
            }
        }

        public async Task UpdateAsync()
        {
            IsBusy = true;

            string web = await DownloadUpcomingJsonAsync();

            if (String.IsNullOrWhiteSpace(web) == false)
            {
                JObject json = ParseIntoJson(web);

                if (json != null)
                {
                    NotifyIfLive(json);

                    ProcessEvents(json);
                }
            }

            IsBusy = false;
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
            catch (JsonReaderException e)
            {
                Utils.LogException(e);

                json = null;
            }

            return json;
        }

        private void NotifyIfLive(JObject json)
        {
            if (json["liveNow"].HasValues)
            {
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

        private void ProcessEvents(JObject json)
        {
            IEnumerable<GBUpcomingEvent> eventsFromWeb = GetEvents(json);

            Events.AddMissing(eventsFromWeb);

            RemoveOld(eventsFromWeb);
        }

        private static IEnumerable<GBUpcomingEvent> GetEvents(JObject json)
        {
            List<GBUpcomingEvent> events = new List<GBUpcomingEvent>();

            IJEnumerable<JToken> upcoming = json["upcoming"];
            
            foreach (JObject each in upcoming)
            {
                GBUpcomingEvent newEvent = null;
                
                if (GBUpcomingEvent.TryCreate(each, out newEvent))
                {
                    events.Add(newEvent);
                }
            }

            return events.Count > 0 ? events : Enumerable.Empty<GBUpcomingEvent>();
        }
        
        private void RemoveOld(IEnumerable<GBUpcomingEvent> eventsFromHtml)
        {
            /*
             * 
             * DO NOT USE EVENTS.CLEAR !!!!!!!!!!
             * 
             * Events.Clear() fires NotifyCollectionChangedEventsArgs.Reset - NOT .Remove
             * 
             * .Reset does not give you a list of what was removed
             *
             */

            List<GBUpcomingEvent> toRemove = (from each in Events
                                              where each.Time.Equals(DateTime.MaxValue) || eventsFromHtml.Contains(each) == false
                                              select each)
                                              .ToList();

            Events.RemoveList(toRemove);
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

            sb.AppendLine(GetType().ToString());
            sb.AppendLine(WindowTitle);
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "IsLive: {0}", IsLive));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Number of events: {0}", Events.Count));

            return sb.ToString();
        }
    }
}