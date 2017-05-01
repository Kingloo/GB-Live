using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
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
                    _refreshCommandAsync = new DelegateCommandAsync(RefreshAsync, CanExecuteAsync);
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
                    _goToHomepageInBrowserCommand = new DelegateCommand(GoToHomepageInBrowser, CanExecute);
                }

                return _goToHomepageInBrowserCommand;
            }
        }

        private static void GoToHomepageInBrowser()
        {
            Uri gbHome = ConfigurationManagerWrapper.GetUriOrThrow(GBHomePageKey);

            Utils.OpenUriInBrowser(gbHome);
        }

        private DelegateCommand _goToChatPageInBrowserCommand = null;
        public DelegateCommand GoToChatPageInBrowserCommand
        {
            get
            {
                if (_goToChatPageInBrowserCommand == null)
                {
                    _goToChatPageInBrowserCommand = new DelegateCommand(GoToChatPageInBrowser, CanExecute);
                }

                return _goToChatPageInBrowserCommand;
            }
        }

        private static void GoToChatPageInBrowser()
        {
            Uri gbChat = ConfigurationManagerWrapper.GetUriOrThrow(GBChatKey);

            Utils.OpenUriInBrowser(gbChat);
        }

        private bool CanExecute(object _) => true;

        private bool CanExecuteAsync(object _) => !IsBusy;
        #endregion

        #region Fields
        private const string appName = "GB Live";
        private const string GBHomePageKey = "GBHomePage";
        private const string GBChatKey = "GBChat";
        private const string GBIsLiveMessageKey = "GBIsLiveMessage";
        private const string GBIsNotLiveMessageKey = "GBIsNotLiveMessage";
        private const string GBUpcomingJsonKey = "GBUpcomingJson";

        private readonly DispatcherTimer updateTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
        {
            Interval = TimeSpan.FromMinutes(3)
        };
        #endregion

        #region Properties
        public string WindowTitle
            => IsBusy
            ? string.Format(CultureInfo.CurrentCulture, "{0}: checking ...", appName)
            : appName;

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
            => RefreshCommandAsync.RaiseCanExecuteChanged();

        private readonly ObservableCollection<GBUpcomingEvent> _events
            = new ObservableCollection<GBUpcomingEvent>();
        public IReadOnlyCollection<GBUpcomingEvent> Events => _events;
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
            Uri gbUpcoming = ConfigurationManagerWrapper.GetUriOrThrow(GBUpcomingJsonKey);
            
            return await Download.WebsiteAsync(gbUpcoming)
                .ConfigureAwait(false);
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
                Log.LogException(ex);
            }

            return json;
        }

        private void NotifyIfLive(JObject json)
        {
            bool isLiveNow = IsLive;

            if (json["liveNow"].HasValues)
            {
                LiveShowName = GetLiveShowName(json["liveNow"]);

                if (!isLiveNow)
                {
                    isLiveNow = true;

                    CreateAndSendLiveNotification();
                }
            }
            else
            {
                isLiveNow = false;
            }

            IsLive = isLiveNow;
        }

        private static void CreateAndSendLiveNotification()
        {
            string liveMessage = ConfigurationManagerWrapper.GetStringOrThrow(GBIsLiveMessageKey);
            Uri gbChat = ConfigurationManagerWrapper.GetUriOrThrow(GBChatKey);

            NotificationService.Send(liveMessage, () => Utils.OpenUriInBrowser(gbChat));
        }

        private static string GetLiveShowName(JToken token)
            => (string)token["title"] ?? "Untitled Live Show";

        private void ProcessEvents(JObject json)
        {
            var eventsFromWeb = GetEvents(json);

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
            var events = new List<GBUpcomingEvent>();
            
            if (json.TryGetValue("upcoming", StringComparison.OrdinalIgnoreCase, out JToken upcoming))
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
            // x.Time == DateTime.MaxValue: the countdown has fired
            // x.Time < DateTime.Now: the event is in the past
            // !eventsFromHtml.Contains(x): event removed before time (e.g. cancelled, rescheduled)

            var toRemove = _events
                .Where(x => x.Time == DateTime.MaxValue || x.Time < DateTime.Now || !eventsFromHtml.Contains(x))
                .ToList();

            _events.RemoveRange(toRemove);
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