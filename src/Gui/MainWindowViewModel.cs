using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using GBLive.Common;
using GBLive.Extensions;
using GBLive.GiantBomb;

namespace GBLive.Gui
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private DispatcherTimer? timer = null;

        private readonly ObservableCollection<UpcomingEvent> _events = new ObservableCollection<UpcomingEvent>();
        public IReadOnlyCollection<UpcomingEvent> Events => _events;

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

        private string _liveShowTitle = Settings.NameOfNoLiveShow;
        public string LiveShowTitle
        {
            get => _liveShowTitle;
            set
            {
                _liveShowTitle = value;

                OnPropertyChanged(nameof(LiveShowTitle));
            }
        }

        public MainWindowViewModel() { }

        public async Task UpdateAsync()
        {
            UpcomingResponse response = await GiantBombService.UpdateAsync();

            if (response.Reason != Reason.Success)
            {
                return;
            }

            bool wasLive = IsLive;

            IsLive = response.IsLive;
            LiveShowTitle = response.LiveShowTitle;

            if (!wasLive && IsLive)
            {
                NotificationService.Send(Settings.IsLiveMessage, GoToChat);
            }

            AddNewRemoveOldRemoveExpired(response.Events);
        }

        private void AddNewRemoveOldRemoveExpired(IEnumerable<UpcomingEvent> events)
        {
            // add events that that we don't already have, and that are in the future

            var toAdd = events
                .Where(x => !_events.Contains(x))
                .Where(x => x.Time > DateTimeOffset.Now)
                .ToList();

            foreach (UpcomingEvent add in toAdd)
            {
                add.StartCountdown();

                _events.Add(add);
            }

            // remove events that we have locally but that are no longer in the API response

            var toRemove = _events
                .Where(x => !events.Contains(x))
                .ToList();

            foreach (UpcomingEvent remove in toRemove)
            {
                remove.StopCountdown();

                _events.Remove(remove);
            }

            // remove any events that the API response contains but whose time is in the past
            // well, 10 minutes in the past to allow for some leeway

            var expired = _events
                .Where(x => x.Time < DateTimeOffset.Now.AddMinutes(-10d))
                .ToList();

            foreach (UpcomingEvent each in expired)
            {
                each.StopCountdown();

                _events.Remove(each);
            }
        }

        public void GoToHome() => Settings.Home.OpenInBrowser();

        public void GoToChat() => Settings.Chat.OpenInBrowser();

        public void StartTimer()
        {
            if (timer is null)
            {
                timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
                {
                    Interval = Settings.UpdateInterval
                };

                timer.Tick += Timer_Tick;
            }

            if (!timer.IsEnabled)
            {
                timer.Start();
            }
        }

        public void StopTimer()
        {
            if (timer is null)
            {
                return;
            }

            timer.Stop();
            timer.Tick -= Timer_Tick;

            timer = null;
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            await UpdateAsync();
        }       
    }
}