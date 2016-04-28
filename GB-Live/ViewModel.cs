using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
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
            await UpdateAllAsync();
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

        private void GoToHomepageInBrowser()
        {
            Utils.OpenUriInBrowser(ConfigurationManager.AppSettings["GBHomepage"]);
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
            Utils.OpenUriInBrowser(ConfigurationManager.AppSettings["GBChat"]);
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

        private readonly DispatcherTimer updateEventsTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 25, 0)
        };

        private readonly DispatcherTimer updateLiveTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 3, 0)
        };
        #endregion

        #region Properties
        public string WindowTitle
        {
            get
            {
                if (IsBusy)
                {
                    return string.Format(CultureInfo.CurrentCulture, "{0}: checking ...", appName);
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

        private readonly ObservableCollection<GBUpcomingEvent> _events = new ObservableCollection<GBUpcomingEvent>();
        public ObservableCollection<GBUpcomingEvent> Events { get { return _events; } }
        #endregion

        public ViewModel()
        {
            Events.CollectionChanged += Events_CollectionChanged;
            
            updateEventsTimer.Tick += async (sender, e) => await UpdateEventsAsync();
            updateEventsTimer.Start();
            
            updateLiveTimer.Tick += async (sender, e) => await UpdateLiveAsync();
            updateLiveTimer.Start();
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
                each.StartCountdownTimer();
            }
        }

        private static void StopAllEventTimers(IList oldItems)
        {
            foreach (GBUpcomingEvent each in oldItems)
            {
                each.StopCountdownTimer();
            }
        }
        
        public async Task UpdateAllAsync()
        {
            IsBusy = true;

            await Task.WhenAll(UpdateEventsAsync(), UpdateLiveAsync());

            IsBusy = false;
        }

        private async Task UpdateEventsAsync()
        {
            HttpWebRequest req = BuildRequest(new Uri(ConfigurationManager.AppSettings["GBHomepage"]));
            string website = await Utils.DownloadWebsiteAsStringAsync(req);

            if (String.IsNullOrWhiteSpace(website)) { return; }

            IEnumerable<GBUpcomingEvent> eventsFromHtml = await RetrieveEventsFromHtmlAsync(website);

            if (eventsFromHtml.Count() > 0)
            {
                Events.AddMissing(eventsFromHtml);
            }

            Remove(eventsFromHtml);
        }

        private async Task UpdateLiveAsync()
        {
            HttpWebRequest req = BuildRequest(new Uri(ConfigurationManager.AppSettings["GBUpcomingJson"]));
            string website = await Utils.DownloadWebsiteAsStringAsync(req);

            if (String.IsNullOrWhiteSpace(website)) { return; }

            JObject json = null;

            try
            {
                json = JObject.Parse(website);
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

                        NameValueCollection appSettings = ConfigurationManager.AppSettings;

                        NotificationService.Send(appSettings["GBIsLiveMessage"], () =>
                        {
                            Utils.OpenUriInBrowser(appSettings["GBChat"]);
                        });
                    }
                }
                else
                {
                    IsLive = false;
                }
            }
        }

        private static async Task<IEnumerable<GBUpcomingEvent>> RetrieveEventsFromHtmlAsync(string website)
        {
            FromBetweenResult res = website.FromBetween(upcomingBegins, upcomingEnds);

            if (res.Result == Result.Success)
            {
                return await ParseHtmlForEventsAsync(res.ResultValue);
            }
            else
            {
                return Enumerable.Empty<GBUpcomingEvent>();
            }
        }

        private static async Task<IEnumerable<GBUpcomingEvent>> ParseHtmlForEventsAsync(string upcomingHtml)
        {
            List<GBUpcomingEvent> events = new List<GBUpcomingEvent>();

            string dd = string.Empty;
            bool shouldConcat = false;

            using (StringReader sr = new StringReader(upcomingHtml))
            {
                string line = string.Empty;

                while ((line = await sr.ReadLineAsync()) != null)
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
                    }

                    if (shouldConcat)
                    {
                        dd += line;
                    }
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
