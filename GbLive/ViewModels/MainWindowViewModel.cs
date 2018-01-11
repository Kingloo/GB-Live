using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using GbLive.Common;
using GbLive.GiantBomb;

namespace GbLive.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly GiantBombService gbService = new GiantBombService();
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

        public static void GoToHome()
            => Utils.OpenUriInBrowser(Settings.Home);

        public static void GoToChat()
            => Utils.OpenUriInBrowser(Settings.Chat);

        public async Task UpdateAsync()
        {
            await gbService.UpdateAsync();

            bool wasLive = IsLive;

            IsLive = gbService.IsLive;

            if (!wasLive && IsLive)
            {
                NotificationService.Send(
                    Settings.IsLiveMessage,
                    () => Utils.OpenUriInBrowser(Settings.Chat));
            }

            LiveShowName = gbService.LiveShowName;

            UpdateEvents(gbService.Events);
        }

        private void UpdateEvents(IReadOnlyList<UpcomingEvent> eventsFromWeb)
        {
            AddNew(eventsFromWeb);
            RemoveOld(eventsFromWeb);
        }

        private void AddNew(IReadOnlyList<UpcomingEvent> eventsFromWeb)
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

        private void RemoveOld(IReadOnlyList<UpcomingEvent> eventsFromWeb)
        {
            var toRemove = _events
                .Where(x => x.Time < DateTime.Now || !eventsFromWeb.Contains(x))
                .ToList();

            foreach (UpcomingEvent each in toRemove)
            {
                if (_events.Contains(each))
                {
                    _events.Remove(each);

                    each.StopCountdown();
                }
            }
        }

        
        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    (gbService as IDisposable).Dispose();
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
