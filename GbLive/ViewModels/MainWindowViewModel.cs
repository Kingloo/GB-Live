using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using GbLive.Common;
using GbLive.GiantBomb;

namespace GbLive.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private DispatcherTimer updateTimer = null;

        #region Properties
        private bool _isLive = false;
        public bool IsLive
        {
            get => _isLive;
            set => SetProperty(ref _isLive, value, nameof(IsLive));
        }

        private string _liveShowName = string.Empty;
        public string LiveShowName
        {
            get => _liveShowName;
            set => SetProperty(ref _liveShowName, value, nameof(LiveShowName));
        }

        private readonly ObservableCollection<UpcomingEvent> _events
            = new ObservableCollection<UpcomingEvent>();
        public IReadOnlyCollection<UpcomingEvent> Events => _events;
        #endregion
        
        public MainWindowViewModel()
        {
            StartUpdateTimer();
        }

        private void StartUpdateTimer()
        {
            updateTimer = new DispatcherTimer
            {
                Interval = Settings.UpdateInterval
            };

            updateTimer.Tick += async (s, e) => await UpdateAsync();

            updateTimer.Start();
        }

        public void GoToHome()
            => Utils.OpenUriInBrowser(Settings.Home);

        public void GoToChat()
            => Utils.OpenUriInBrowser(Settings.Chat);

        public async Task UpdateAsync()
        {
            bool wasLive = IsLive;

            UpcomingResponse response = await GiantBombService.UpdateAsync();

            IsLive = response.IsLive;
            
            if (!wasLive && IsLive)
            {
                NotificationService.Send(
                    Settings.IsLiveMessage,
                    () => Utils.OpenUriInBrowser(Settings.Chat));
            }
            
            LiveShowName = response.LiveShowName;

            AddNewEvents(response.Events);

            RemoveOldEvents(response.Events);
        }
        
        private void AddNewEvents(IReadOnlyList<UpcomingEvent> eventsFromWeb)
        {
            foreach (UpcomingEvent each in eventsFromWeb)
            {
                if (!_events.Contains(each))
                {
                    each.StartCountdown();

                    _events.Add(each);
                }
            }
        }

        private void RemoveOldEvents(IReadOnlyList<UpcomingEvent> eventsFromWeb)
        {
            var toRemove = _events
                .Where(x => x.Time < DateTime.Now || !eventsFromWeb.Contains(x))
                .ToList();

            foreach (UpcomingEvent each in toRemove)
            {
                if (_events.Contains(each))
                {
                    each.StopCountdown();

                    _events.Remove(each);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "IsLive: {0}", IsLive.ToString(CultureInfo.CurrentCulture)));
            sb.AppendLine(LiveShowName);
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "No. of Events: {0}", Events.Count.ToString()));

            if (Events.Count > 0)
            {
                sb.AppendLine("Events: ");

                foreach (UpcomingEvent each in Events)
                {
                    sb.Append(each.ToString());
                }
            }
            
            return sb.ToString();
        }
    }
}
