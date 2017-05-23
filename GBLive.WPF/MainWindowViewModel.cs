using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using GBLive.WPF.Common;
using GBLive.WPF.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GBLive.WPF
{
    public class MainWindowViewModel : IViewModel, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        
        #region Fields
        private readonly IWebRetriever jsonRetriever = null;
        private DispatcherTimer updateTimer = default(DispatcherTimer);
        #endregion

        #region Properties
        private readonly GBSettings _settings = default(GBSettings);
        public GBSettings Settings => _settings;

        private bool _isLive = false;
        public bool IsLive
        {
            get => _isLive;
            set
            {
                _isLive = value;

                OnPropertyChanged(nameof(IsLive));
            }
        }
        
        private string _liveShowName = string.Empty;
        public string LiveShowName
        {
            get
            {
                return IsLive ? _liveShowName : "No live show";
            }
            set
            {
                _liveShowName = value;

                OnPropertyChanged(nameof(LiveShowName));
            }
        }

        private readonly ObservableCollection<GBUpcomingEvent> _events
            = new ObservableCollection<GBUpcomingEvent>();
        public IReadOnlyCollection<GBUpcomingEvent> Events => _events;
        #endregion
        
        public MainWindowViewModel(GBSettings settings, IWebRetriever jsonRetriever, bool autostart = false)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.jsonRetriever = jsonRetriever ?? throw new ArgumentNullException(nameof(jsonRetriever));

            _events.CollectionChanged += _events_CollectionChanged;

            if (autostart)
            {
                StartTimer();
            }
        }

        private void _events_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // NEVER use .Reset
            // .Reset does not provide a list of what was removed
            // we need such a list so as to be able to unhook the countdown timer events
            // failing to do so could result in memory leak

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnEventAdded(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnEventRemoved(e.OldItems);
                    break;
                default:
                    break;
            }
        }

        private void OnEventAdded(IList newItems)
        {
            var newEvents = newItems.Cast<GBUpcomingEvent>();

            foreach (var each in newEvents)
            {
                each.CountdownFired += GBUpcomingEventCountdown_Fired;

                each.StartCountdownTimer();
            }
        }

        private void GBUpcomingEventCountdown_Fired(object sender, BasicEventArgs<string> e)
        {
            NotificationService.Send(e.Object, () => GoToHomePage());
        }

        private static void OnEventRemoved(IList oldItems)
        {
            var removedEvents = oldItems.Cast<GBUpcomingEvent>();

            foreach (var each in removedEvents)
            {
                each.StopCountdownTimer();
            }
        }

        public void StartTimer()
        {
            updateTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
            {
                Interval = Settings.UpdateInterval
            };

            updateTimer.Tick += async (s, e) => await UpdateAsync();
            updateTimer.Start();
        }
        
        public async Task UpdateAsync()
        {
            (string rawJson, HttpStatusCode status) = await jsonRetriever.GetAsync();
            
            if (status != HttpStatusCode.OK)
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, "GBJson request failed: {0}", status);

                await Log.LogMessageAsync(errorMessage);

                return;
            }

            if (String.IsNullOrWhiteSpace(rawJson)) { return; }
            
            JObject json = ParseIntoJson(rawJson);

            if (json == null) { return; }

            NotifyIfLive(json);

            ProcessEvents(json);
        }

        private static JObject ParseIntoJson(string rawJson)
        {
            JObject json = null;

            try
            {
                json = JObject.Parse(rawJson);
            }
            catch (JsonReaderException ex)
            {
                Log.LogException(ex);
            }

            return json;
        }

        private void NotifyIfLive(JObject json)
        {
            if (json["liveNow"].HasValues)
            {
                LiveShowName = GetLiveShowName(json["liveNow"]);
                
                if (!IsLive)
                {
#if !DEBUG
                    SendNotification();
#endif
                }

                IsLive = true;
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

        private void SendNotification()
        {
            NotificationService.Send(Settings.IsLiveMessage, GoToChatPage);
        }

        public void GoToHomePage()
        {
            Utils.OpenUriInBrowser(Settings.GBHomePage);
        }

        public void GoToChatPage()
        {
            Utils.OpenUriInBrowser(Settings.GBChatPage);
        }
        
        private void ProcessEvents(JObject json)
        {
            var eventsFromWeb = CreateEvents(json);

            foreach (GBUpcomingEvent each in eventsFromWeb)
            {
                if (!_events.Contains(each) && (each.Time > DateTime.Now))
                {
                    _events.Add(each);
                }
            }

            RemoveExpired(eventsFromWeb);
        }

        private static IEnumerable<GBUpcomingEvent> CreateEvents(JObject json)
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

            return events.Any() ? events : Enumerable.Empty<GBUpcomingEvent>();
        }

        private void RemoveExpired(IEnumerable<GBUpcomingEvent> eventsFromWeb)
        {
            /*
             Events are to be removed when
               a) Time is set to MaxValue, see FormatDateTimeString
               b) Time is in the past, aka < DateTime.Now
               c) the event is not in the JSON returned from the site
             */

            var toRemove = _events
                .Where(x => x.Time == DateTime.MaxValue || x.Time < DateTime.Now || !eventsFromWeb.Contains(x))
                .ToList();

            // NEVER use .Reset
            // .Reset does not provide a list of what was removed
            // we need such a list so as to be able to unhook the countdown timer events
            // failing to do so could result in memory leak

            _events.RemoveRange(toRemove);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().Name);
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "There are {0} events", Events.Count));
            sb.AppendLine(IsLive.ToString(CultureInfo.CurrentCulture));
            sb.AppendLine(LiveShowName);
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Updates occur every {0} seconds", updateTimer.Interval.TotalSeconds));

            return sb.ToString();
        }
    }
}
