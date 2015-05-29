using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
            await UpdateAsync().ConfigureAwait(false);
        }

        private DelegateCommand _goToHomePageInBrowserCommand = null;
        public DelegateCommand GoToHomePageInBrowserCommand
        {
            get
            {
                if (_goToHomePageInBrowserCommand == null)
                {
                    _goToHomePageInBrowserCommand = new DelegateCommand(GoToHomePageInBrowser, canExecute);
                }

                return _goToHomePageInBrowserCommand;
            }
        }

        private void GoToHomePageInBrowser()
        {
            Utils.OpenUriInBrowser(Globals.gbHome);
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

        private void GoToChatPageInBrowser()
        {
            Utils.OpenUriInBrowser(Globals.gbChat);
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

        private void Exit()
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
        private const string upcomingBegins = "<dl class=\"promo-upcoming\">";
        private const string upcomingEnds = "</dl>";
        private DispatcherTimer updateTimer = null;
        #endregion

        #region Properties
        public string WindowTitle
        {
            get
            {
                if (IsBusy)
                {
                    return string.Format("{0}: checking ...", appName);
                }
                else
                {
                    return appName;
                }
            }
        }

        private bool _isLive = false;
        public bool IsLive
        {
            get { return this._isLive; }
            set
            {
                if (!value == _isLive) // only need to change anything if value and the field are different
                {
                    this._isLive = value;

                    OnNotifyPropertyChanged();
                }
            }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get
            {
                return this._isBusy;
            }
            set
            {
                if (!value == _isBusy) // only need to change anything if value and the field are different
                {
                    this._isBusy = value;

                    OnNotifyPropertyChanged();
                    OnNotifyPropertyChanged("WindowTitle");

                    RaiseAllCommandCanExecuteChanged();
                }
            }
        }

        private void RaiseAllCommandCanExecuteChanged()
        {
            this.RefreshCommandAsync.RaiseCanExecuteChanged();
        }

        private ObservableCollection<GBUpcomingEvent> _events = new ObservableCollection<GBUpcomingEvent>();
        public ObservableCollection<GBUpcomingEvent> Events { get { return _events; } }
        #endregion

        public ViewModel()
        {
            Application.Current.MainWindow.Loaded += MainWindow_Loaded;
            Events.CollectionChanged += Events_CollectionChanged;

            updateTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 4, 0), // 0 hours, 4 minutes, 0 seconds
            };

            updateTimer.Tick += updateTimer_Tick;
            updateTimer.Start();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateAsync().ConfigureAwait(false);
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

        private void StartAllEventTimers(IList newItems)
        {
            foreach (GBUpcomingEvent each in newItems)
            {
                each.StartCountdownTimer();
            }
        }

        private void StopAllEventTimers(IList oldItems)
        {
            foreach (GBUpcomingEvent each in oldItems)
            {
                each.StopCountdownTimer();
            }
        }

        private async void updateTimer_Tick(object sender, EventArgs e)
        {
            await UpdateAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync()
        {
            this.IsBusy = true;

            await Task.WhenAll(UpdateEventsAsync(), IsGiantBombLiveAsync());

            this.IsBusy = false;
        }

        private async Task UpdateEventsAsync()
        {
            HttpWebRequest req = BuildHttpWebRequest(Globals.gbHome);
            string websiteAsString = await Utils.DownloadWebsiteAsStringAsync(req).ConfigureAwait(false);

            if (String.IsNullOrWhiteSpace(websiteAsString)) return;

            List<GBUpcomingEvent> eventsFromHtml = RetrieveEventsFromHtml(websiteAsString);

            Utils.SafeDispatcher(() =>
                {
                    if (eventsFromHtml.Count > 0)
                    {
                        Events.AddMissing<GBUpcomingEvent>(eventsFromHtml);
                    }

                    Remove(eventsFromHtml);
                },
                DispatcherPriority.Background);
        }

        private async Task IsGiantBombLiveAsync()
        {
            HttpWebRequest req = BuildHttpWebRequest(Globals.gbUpcomingJson);
            string websiteAsString = await Utils.DownloadWebsiteAsStringAsync(req).ConfigureAwait(false);

            if (String.IsNullOrWhiteSpace(websiteAsString)) return;

            JObject json = null;

            try
            {
                json = JObject.Parse(websiteAsString);
            }
            catch (JsonReaderException e)
            {
                Utils.LogException(e);

                return;
            }

            if (json != null)
            {
                if (json["liveNow"].HasValues)
                {
                    if (IsLive == false)
                    {
                        IsLive = true;

                        Utils.SafeDispatcher(() =>
                            NotificationService.Send("GiantBomb is LIVE", Globals.gbChat),
                            DispatcherPriority.Background);
                    }
                }
                else
                {
                    IsLive = false;
                }
            }
        }

        private void UpdateUpcomingEvents(string websiteAsString)
        {
            List<GBUpcomingEvent> eventsFromHtml = RetrieveEventsFromHtml(websiteAsString);

            if (eventsFromHtml.Count > 0)
            {
                Events.AddMissing<GBUpcomingEvent>(eventsFromHtml);
            }

            Remove(eventsFromHtml);
        }

        private List<GBUpcomingEvent> RetrieveEventsFromHtml(string websiteAsString)
        {
            FromBetweenResult res = websiteAsString.FromBetween(upcomingBegins, upcomingEnds);

            if (res.Result == Result.Success)
            {
                return ParseHtmlForEvents(res.ResultString);
            }
            else
            {
                return new List<GBUpcomingEvent>(0);
            }
        }

        private List<GBUpcomingEvent> ParseHtmlForEvents(string upcomingHtml)
        {
            List<GBUpcomingEvent> events = new List<GBUpcomingEvent>();

            string dd = string.Empty;
            bool shouldConcat = false;

            using (StringReader sr = new StringReader(upcomingHtml))
            {
                string line = string.Empty;
                StringBuilder sb_Log = new StringBuilder();

                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line.StartsWith("<dd"))
                    {
                        shouldConcat = true;
                    }
                    else if (line.StartsWith("</dd"))
                    {
                        shouldConcat = false;

                        GBUpcomingEvent newEvent = null;

                        if (GBUpcomingEvent.TryCreate(dd, out newEvent))
                        {
                            if (newEvent.Time > DateTime.Now)
                            {
                                events.Add(newEvent);
                            }

                            dd = string.Empty;
                        }
                        else
                        {
                            sb_Log.AppendLine(string.Format("Event could not be created from:{0}{1}", Environment.NewLine, dd));
                        }
                    }

                    if (shouldConcat)
                    {
                        dd += line;
                    }
                }

                if (sb_Log.Length > 0)
                {
                    Utils.LogMessage(sb_Log.ToString());
                }
            }

            return events;
        }

        private void Remove(IEnumerable<GBUpcomingEvent> eventsFromHtml)
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
                                              .ToList<GBUpcomingEvent>();

            Events.RemoveList<GBUpcomingEvent>(toRemove);
        }

        private HttpWebRequest BuildHttpWebRequest(Uri gbUri)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(gbUri);

            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            req.Host = gbUri.DnsSafeHost;
            req.KeepAlive = false;
            req.Method = "GET";
            req.ProtocolVersion = HttpVersion.Version11;
            req.Referer = string.Format("{0}://{1}/", gbUri.GetLeftPart(UriPartial.Scheme), gbUri.DnsSafeHost);
            req.Timeout = 2500;
            req.UserAgent = "IE11: Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

            req.Headers.Add("DNT", "1");
            req.Headers.Add("Accept-Encoding", "gzip, deflate");

            // 1) lu is probably country-code, returned timezone has yet to be wrong after setting this
            // 2) after months of use, the prior claim remains true
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.Add(gbUri, new Cookie("xcg", "lu"));

            return req;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(this.GetType().ToString());
            sb.AppendLine(WindowTitle);
            sb.AppendLine(string.Format("IsLive: {0}", IsLive));
            sb.AppendLine(string.Format("Number of events: {0}", Events.Count));

            return sb.ToString();
        }
    }
}
