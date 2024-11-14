using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GBLive.Common;
using GBLive.GiantBomb;
using GBLive.Options;
using GBLive.Exceptions;

namespace GBLive.Gui
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private void OnPropertyChanged(string propertyName)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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

		private readonly ILogger<MainWindowViewModel> logger;

		private readonly ObservableCollection<Show> shows = new ObservableCollection<Show>();
		public IReadOnlyCollection<Show> Shows { get => shows; }

		private readonly IOptionsMonitor<GiantBombOptions> giantBombOptionsMonitor;
		public GiantBombOptions GiantBombOptions { get => giantBombOptionsMonitor.CurrentValue; }

		private readonly IOptionsMonitor<GBLiveOptions> gbLiveOptionsMonitor;
		public GBLiveOptions GBLiveOptions { get => gbLiveOptionsMonitor.CurrentValue; }

		public MainWindowViewModel(
			ILogger<MainWindowViewModel> logger,
			IOptionsMonitor<GiantBombOptions> giantBombOptionsMonitor,
			IOptionsMonitor<GBLiveOptions> gbLiveOptionsMonitor)
		{
			this.logger = logger;
			this.giantBombOptionsMonitor = giantBombOptionsMonitor;
			this.gbLiveOptionsMonitor = gbLiveOptionsMonitor;
		}

		public ValueTask UpdateAsync()
			=> UpdateAsync(CancellationToken.None);

		public async ValueTask UpdateAsync(CancellationToken cancellationToken)
		{
			GiantBombOptions currentGiantBombOptions = giantBombOptionsMonitor.CurrentValue;
			GBLiveOptions currentGBLiveOptions = gbLiveOptionsMonitor.CurrentValue;

			UpcomingData? upcomingData = await Updater.UpdateAsync(
				currentGBLiveOptions,
				currentGiantBombOptions,
				cancellationToken)
			.ConfigureAwait(true);

			logger.LogDebug(
				"updated - {}, {} upcoming {}",
				upcomingData?.LiveNow is not null ? "live" : "not live",
				upcomingData?.Upcoming.Count ?? 0,
				upcomingData?.Upcoming.Count == 1 ? "event" : "events"
			);

			if (upcomingData is null)
			{
				IsLive = false;
				LiveShowTitle = currentGBLiveOptions.NameOfNoLiveShow;

				return;
			}

			bool wasLive = IsLive;

			IsLive = upcomingData.LiveNow is not null;

			LiveShowTitle = upcomingData.LiveNow is not null
				? upcomingData.LiveNow.Title
				: currentGBLiveOptions.NameOfNoLiveShow;

			if (currentGBLiveOptions.ShowNotifications)
			{
				if (!wasLive && IsLive) // we only want the notification once, upon changing from not-live to live
				{
					NotificationService.Send(currentGBLiveOptions.IsLiveMessage, OpenChatPage);
				}
			}

			AddNewShows(upcomingData);

			RemoveRemovedShows(upcomingData);

			RemoveOldShows(olderThan: TimeSpan.FromSeconds(15d));
		}

		private void AddNewShows(UpcomingData upcomingData)
		{
			IEnumerable<Show> futureShows = upcomingData.Upcoming.Where(x => x.Date > DateTimeOffset.Now);

			foreach (Show show in futureShows)
			{
				if (!shows.Contains(show))
				{
#pragma warning disable IDE0061
					void notify() => NotificationService.Send(show.Title, OpenHomePage);
#pragma warning restore IDE0061

					show.StartCountdown(notify);

					shows.Add(show);
				}
			}
		}

		private void RemoveRemovedShows(UpcomingData upcomingData)
		{
			// remove events that we have locally but that are no longer in the API response

			IList<Show> showsNoLongerInApi = shows
				.Where(x => x.Date > DateTimeOffset.Now.AddSeconds(15d)) // to avoid removing an active/will-happen Show that hasn't sent its notification yet
				.Where(x => !upcomingData.Upcoming.Contains(x))
				.ToList();

			RemoveShows(showsNoLongerInApi);
		}

		private void RemoveOldShows(TimeSpan olderThan)
		{
			IList<Show> showsOlderThan = shows
				.Where(x => x.Date < DateTimeOffset.Now.Subtract(olderThan))
				.ToList();

			RemoveShows(showsOlderThan);
		}

		private void RemoveShows(IList<Show> toRemove)
		{
			foreach (Show show in toRemove)
			{
				if (shows.Contains(show))
				{
					show.StopCountdown();

					_ = shows.Remove(show);
				}
			}
		}

		public void OpenHomePage() => OpenUriInBrowser(giantBombOptionsMonitor.CurrentValue.HomePage);

		public void OpenChatPage() => OpenUriInBrowser(giantBombOptionsMonitor.CurrentValue.ChatPage);

		private static void OpenUriInBrowser(Uri? uri)
		{
			if (uri is null)
			{
				throw new GBLiveException("uri was null");
			}

			if (!SystemLaunch.Uri(uri))
			{
				throw new ArgumentException($"failed to launch URI: {uri.AbsoluteUri}", nameof(uri));
			}
		}

		public void StartTimer()
		{
			if (timer is null)
			{
				timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
				{
					Interval = gbLiveOptionsMonitor.CurrentValue.UpdateInterval
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
			if (timer is not null)
			{
				timer.Stop();

				timer.Tick -= Timer_Tick;

				timer = null;
			}
		}

		private async void Timer_Tick(object? sender, EventArgs e)
		{
			await UpdateAsync().ConfigureAwait(true);
		}
	}
}
