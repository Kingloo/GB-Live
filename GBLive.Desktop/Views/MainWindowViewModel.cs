using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using GBLive.Desktop.Common;
using GBLive.Desktop.GiantBomb;

namespace GBLive.Desktop.Views
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
        private readonly IGiantBombService _gbService = null;
        private DispatcherTimer updateTimer = default(DispatcherTimer);
        #endregion

        #region Properties
        public bool IsLive => _gbService.IsLive;
        public string LiveShowName => _gbService.LiveShowName;
        public IReadOnlyCollection<UpcomingEvent> Events => _gbService.Events;
        #endregion
        
        public MainWindowViewModel(IGiantBombService gbService)
        {
            _gbService = gbService ?? throw new ArgumentNullException(nameof(gbService));
        }

        public MainWindowViewModel(IGiantBombService gbService, bool autoStart)
            : this(gbService)
        {
            if (autoStart)
            {
                StartUpdateTimer();
            }
        }

        public void StartUpdateTimer()
        {
            updateTimer = new DispatcherTimer() { Interval = Settings.UpdateInterval };

            updateTimer.Tick += async (s, e) => await UpdateAsync();

            updateTimer.Start();
        }
        
        public async Task UpdateAsync()
        {
            bool wasLive = IsLive;

            await _gbService.UpdateAsync();

            if (!wasLive && IsLive)
            {
                NotificationService.Send(
                    Settings.IsLiveMessage,
                    () => Utils.OpenUriInBrowser(Settings.ChatPage));
            }

            TriggerAllPropertyChanges();
        }

        private void TriggerAllPropertyChanges()
        {
            OnPropertyChanged(nameof(IsLive));
            OnPropertyChanged(nameof(LiveShowName));
        }

        public void GoToHomepage() => GoToUri(Settings.Homepage);

        public void GoToChatPage() => GoToUri(Settings.ChatPage);

        private static void GoToUri(Uri uri) => Utils.OpenUriInBrowser(uri);

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "There are {0} events", Events.Count));
            sb.AppendLine(IsLive.ToString(CultureInfo.CurrentCulture));
            sb.AppendLine(LiveShowName);
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Updates occur every {0} seconds", updateTimer.Interval.TotalSeconds));

            return sb.ToString();
        }
    }
}
