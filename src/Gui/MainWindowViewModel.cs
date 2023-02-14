using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using GBLive.Common;
using GBLive.GiantBomb;

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

		private readonly ObservableCollection<Show> shows = new ObservableCollection<Show>();
		public IReadOnlyCollection<Show> Shows { get => shows; }

		public MainWindowViewModel() { }

		public ValueTask UpdateAsync()
			=> UpdateAsync(CancellationToken.None);

		public async ValueTask UpdateAsync(CancellationToken cancellationToken)
		{
			UpcomingData? upcomingData = await Updater.UpdateAsync(Constants.UpcomingJsonUri, cancellationToken).ConfigureAwait(true);

			if (upcomingData is null)
			{
				IsLive = false;
				LiveShowTitle = Constants.NameOfNoLiveShow;

				return;
			}

			bool wasLive = IsLive;

			IsLive = upcomingData.LiveNow is not null;

			LiveShowTitle = upcomingData.LiveNow is not null
				? upcomingData.LiveNow.Title
				: Constants.NameOfNoLiveShow;

			if (!wasLive && IsLive) // we only want the notification once, upon changing from not-live to live
			{
				NotificationService.Send(Constants.IsLiveMessage, OpenChatPage);
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
					void notify() => NotificationService.Send(show.Title, OpenHomePage);

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

					shows.Remove(show);
				}
			}
		}

#pragma warning disable CA1822
		public void OpenHomePage() => OpenUriInBrowser(Constants.HomePage);
#pragma warning restore CA1822

#pragma warning disable CA1822
		public void OpenChatPage() => OpenUriInBrowser(Constants.ChatPage);
#pragma warning restore CA1822

		private static void OpenUriInBrowser(Uri uri)
		{
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
					Interval = Constants.UpdateInterval
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
