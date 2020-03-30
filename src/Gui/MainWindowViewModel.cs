using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using GBLive.Common;
using GBLive.GiantBomb;
using GBLive.GiantBomb.Interfaces;
using Microsoft.Extensions.Options;

namespace GBLive.Gui
{
    public class MainWindowViewModel : IMainWindowViewModel, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private readonly ILogClass _logger;
        private readonly IGiantBombContext _gbContext;
        private readonly IOptions<Settings> _settings;

        private DispatcherTimer? timer = null;
        
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

        private string _liveShowTitle = string.Empty;
        public string LiveShowTitle
        {
            get => _liveShowTitle;
            set
            {
                _liveShowTitle = value;

                OnPropertyChanged(nameof(LiveShowTitle));
            }
        }

        private readonly ObservableCollection<IShow> _shows = new ObservableCollection<IShow>();
        public IReadOnlyCollection<IShow> Shows => _shows;

        public MainWindowViewModel(ILogClass logger, IGiantBombContext gbContext, IOptions<Settings> settings)
        {
            _logger = logger;
            _gbContext = gbContext;
            _settings = settings;
        }

        public async Task UpdateAsync()
        {
            await _logger.MessageAsync("update started", Severity.Debug);

            IResponse response = await _gbContext.UpdateAsync();

            await _logger.MessageAsync($"response contains {response.Shows.Count} shows", Severity.Debug);

            if (response.Reason != Reason.Success)
            {
                await _logger.MessageAsync($"update failed: {response.Reason}", Severity.Warning);

                return;
            }

            bool wasLive = IsLive;

            IsLive = response.IsLive;
            LiveShowTitle = IsLive ? response.LiveShowTitle : _settings.Value.NameOfNoLiveShow;
            
            if (_settings.Value.ShouldNotify)
            {
                if (!wasLive && IsLive) // we only want the notification once, upon changing from not-live to live
                {
                    NotificationService.Send(_settings.Value.IsLiveMessage, OpenChatPage);

                    await _logger.MessageAsync($"GiantBomb went live ({response.LiveShowTitle})", Severity.Information);
                }
            }

            AddNewRemoveOldRemoveExpired(response.Shows);

            await _logger.MessageAsync("update succeeded", Severity.Debug);
        }

        private void AddNewRemoveOldRemoveExpired(IEnumerable<IShow> shows)
        {
            // add events that that we don't already have, and that are in the future

            var toAdd = shows
                .Where(x => !_shows.Contains(x))
                .Where(x => x.Time > DateTimeOffset.Now)
                .ToList();

            if (toAdd.Any())
            {
                foreach (IShow add in toAdd)
                {
                    void notify() => NotificationService.Send(add.Title, OpenHomePage);

                    add.StartCountdown(notify);

                    _shows.Add(add);

                    _logger.Message($"added show {add}", Severity.Debug);
                }
            }
            else
            {
                _logger.Message($"no shows to add", Severity.Debug);
            }
            

            // remove events that we have locally but that are no longer in the API response

            var toRemove = _shows
                .Where(x => !shows.Contains(x))
                .ToList();

            if (toRemove.Any())
            {
                foreach (IShow remove in toRemove)
                {
                    remove.StopCountdown();

                    _shows.Remove(remove);

                    _logger.Message($"removed show {remove}", Severity.Debug);
                }
            }
            else
            {
                _logger.Message($"no shows removed", Severity.Debug);
            }
            

            // remove any events that the API response contains but whose time is in the past
            // well, 10 minutes in the past to allow for some leeway

            var expired = _shows
                .Where(x => x.Time < DateTimeOffset.Now.AddMinutes(-10d))
                .ToList();

            if (expired.Any())
            {
                foreach (IShow each in expired)
                {
                    each.StopCountdown();

                    _shows.Remove(each);

                    _logger.Message($"removed old show {each}", Severity.Debug);
                }
            }
            else
            {
                _logger.Message($"no expired shows", Severity.Debug);
            }
        }

        public void OpenHomePage()
        {
            if (_settings.Value.Home is Uri uri)
            {
                if (!SystemLaunch.Uri(uri))
                {
                    _logger.Message("home page () failed to open", Severity.Error);
                }
            }
            else
            {
                _logger.Message("home page Uri is null", Severity.Error);
            }
        }

        public void OpenChatPage()
        {
            if (_settings.Value.Chat is Uri uri)
            {
                if (!SystemLaunch.Uri(uri))
                {
                    _logger.Message("chat page () failed to open", Severity.Error);
                }
            }
            else
            {
                _logger.Message("chat page Uri is null", Severity.Error);
            }
        }

        public void StartTimer()
        {
            if (timer is null)
            {
                timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
                {
                    Interval = TimeSpan.FromSeconds(_settings.Value.UpdateIntervalInSeconds)
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
            if (!(timer is null))
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;

                timer = null;
            }
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            await UpdateAsync();
        }

        
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _gbContext?.Dispose();
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