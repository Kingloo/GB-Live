using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using GBLive.WPF.Common;
using GBLive.WPF.GiantBomb;

namespace GBLive.WPF.GUI
{
    public interface IMainWindowViewModel : INotifyPropertyChanged
    {
        bool IsLive { get; }
        string LiveShowName { get; }
        IReadOnlyCollection<UpcomingEvent> Events { get; }
        bool IsUpdateTimerRunning { get; }
        TimeSpan UpdateInterval { get; }

        void StartTimer();
        void StopTimer();
        void GoToHome();
        void GoToChat();

        Task UpdateAsync();
    }

    public class MainWindowViewModel : IMainWindowViewModel, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private DispatcherTimer updateTimer = null;

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

        public TimeSpan UpdateInterval => updateTimer == null ? TimeSpan.Zero : updateTimer.Interval;
        #endregion

        public MainWindowViewModel() { }

        public MainWindowViewModel(bool autoStartTimer)
        {
            if (autoStartTimer)
            {
                StartTimer();
            }
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

        public void GoToHome() => Utils.OpenUriInBrowser(Settings.Home);

        public void GoToChat() => Utils.OpenUriInBrowser(Settings.Chat);

        public Task UpdateAsync()
        {
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine(IsLive ? "IsLive: true" : "IsLive: false");
            sb.AppendLine($"Live show name: {LiveShowName}");
            sb.AppendLine($"Number of events: {Events.Count}");
            sb.AppendLine(IsUpdateTimerRunning ? "Timer running: true" : "Timer running: false");
            sb.AppendLine($"Update interval: {UpdateInterval.TotalSeconds}");

            return sb.ToString();
        }

        
        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StopTimer();
                }

                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion
    }
}
