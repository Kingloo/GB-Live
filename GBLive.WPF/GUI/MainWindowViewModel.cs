using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using GBLive.Extensions;
using GBLive.WPF.Common;
using GBLive.WPF.GiantBomb;

namespace GBLive.WPF.GUI
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("property name was null or empty", nameof(propertyName));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private DispatcherTimer updateTimer = null;
        private GiantBombService gbService = new GiantBombService();

        #region Properties
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

        private string _liveShowName = Settings.NameOfNoLiveShow;
        public string LiveShowName
        {
            get => _liveShowName;
            set
            {
                _liveShowName = value;

                OnPropertyChanged(nameof(LiveShowName));
            }
        }

        private readonly ObservableCollection<UpcomingEvent> _events
            = new ObservableCollection<UpcomingEvent>();
        public IReadOnlyCollection<UpcomingEvent> Events => _events;

        public bool IsUpdateTimerRunning => updateTimer != null && updateTimer.IsEnabled;

        public TimeSpan UpdateInterval => updateTimer != null ? updateTimer.Interval : TimeSpan.Zero;
        #endregion

        public MainWindowViewModel() { }

        public MainWindowViewModel(GiantBombService service)
            : this(service, false)
        { }

        public MainWindowViewModel(bool autoStartTimer)
        {
            if (autoStartTimer)
            {
                StartTimer();
            }
        }

        public MainWindowViewModel(GiantBombService service, bool autoStartTimer)
            : this(autoStartTimer)
        {
            gbService = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void SetService(GiantBombService service)
        {
            gbService = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void StartTimer()
        {
            updateTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
            {
                Interval = Settings.UpdateInterval
            };

            updateTimer.Tick += Timer_Tick;

            updateTimer.Start();
        }

        private async void Timer_Tick(object sender, EventArgs e) => await UpdateAsync();

        public void StopTimer()
        {
            if (updateTimer != null)
            {
                updateTimer.Stop();
                updateTimer.Tick -= Timer_Tick;
                updateTimer = null;
            }
        }

        public void GoToHome() => GiantBombService.GoToHome();

        public void GoToChat() => GiantBombService.GoToChat();

        public async Task UpdateAsync()
        {
            UpcomingResponse response = await gbService.UpdateAsync();

            if (response.IsSuccessful)
            {
                bool wasLive = IsLive;

                IsLive = response.IsLive;

                if (!wasLive && IsLive)
                {
                    NotificationService.Send(Settings.IsLiveMessage, GoToChat);
                }

                LiveShowName = response.LiveShowName;

                AddNewEvents(response.Events);

                // events that are no longer in the API response
                RemoveCertainEvents(
                    from each in _events
                    where !response.Events.Contains(each)
                    select each);
            }

            // events whose .Time is now in the past
            RemoveCertainEvents(
                from each in _events
                where each.Time < DateTimeOffset.Now
                select each);
        }

        private void AddNewEvents(IReadOnlyList<UpcomingEvent> events)
        {
            foreach (UpcomingEvent each in events)
            {
                if (!_events.Contains(each))
                {
                    each.StartCountdown();

                    _events.Add(each);
                }
            }
        }

        private void RemoveCertainEvents(IEnumerable<UpcomingEvent> events)
        {
            var toRemove = events.ToList();

            foreach (UpcomingEvent each in toRemove)
            {
                if (_events.Contains(each))
                {
                    each.StopCountdown();

                    _events.Remove(each);
                }
            }
        }

        public Task LogEverythingAsync() => Log.LogMessageAsync(ToString());

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine(IsLive ? "IsLive: true" : "IsLive: false");
            sb.AppendLine($"Live show name: {LiveShowName}");
            sb.AppendLine($"Number of events: {Events.Count}");
            sb.AppendLine(IsUpdateTimerRunning ? "Timer running: true" : "Timer running: false");
            sb.AppendLine($"Update interval: {UpdateInterval.TotalSeconds} seconds");

            if (Events.Count > 0)
            {
                sb.AppendLine();

                foreach (UpcomingEvent each in Events)
                {
                    sb.AppendLine(each.ToString());
                }
            }

            return sb.ToString();
        }
    }
}
