using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using GBLive.Common;
using GBLive.JsonConverters;

namespace GBLive.GiantBomb
{
	public class Show
	{
		private DispatcherCountdownTimer? countdown;

		public string Type { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;

		[JsonConverter(typeof(JsonUriConverter))]
		public Uri? Image { get; set; }

		[JsonConverter(typeof(JsonDateTimeOffsetConverter))]
		public DateTimeOffset Date { get; set; } = DateTimeOffset.MinValue;

		[JsonPropertyName("premium")]
		public bool IsPremium { get; set; } = false;

		public Show() { }

		public void StartCountdown(Action notify)
		{
			if (countdown != null && countdown.IsRunning)
			{
				return;
			}

			TimeSpan timeUntilShowIncludingBuffer = (Date - DateTimeOffset.Now)
				.Add(TimeSpan.FromSeconds(10d)); // some buffer time to allow the site to actually update

			if (timeUntilShowIncludingBuffer > TimeSpan.FromDays(24d))
			{
				/*
					you can start a DispatcherTimer with a ticks of type Int64,
					however, the equivalent number of milliseconds cannot exceed Int32.MaxValue
					Int32.MaxValue's worth of milliseconds is ~ 24.58 days

					https://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Threading/DispatcherTimer.cs

					or

					https://github.com/dotnet/wpf/blob/master/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Threading/DispatcherTimer.cs

					-> ctor with TimeSpan, DispatcherPriority, EventHandler, Dispatcher
				*/

				return;
			}

			countdown = new DispatcherCountdownTimer(timeUntilShowIncludingBuffer, notify);

			countdown.Start();
		}

		public void StopCountdown()
		{
			if (countdown is not null)
			{
				countdown.Stop();

				countdown = null;
			}
		}

		public int CompareTo([AllowNull] Show other) => (other is not null) ? Date.CompareTo(other.Date) : 1; // we treat null-other as always earlier

		public bool Equals([AllowNull] Show other) => (other is not null) && EqualsInternal(other);

		private bool EqualsInternal(Show show)
		{
			var sco = StringComparison.Ordinal;

			bool sameTitle = Title.Equals(show.Title, sco);
			bool sameTime = Date.EqualsExact(show.Date);
			bool samePremium = IsPremium == show.IsPremium;
			bool sameShowType = Type.Equals(show.Type, sco);
			bool sameImage = AreLinksEqual(Image, show.Image);

			return sameTitle && sameTime && samePremium && sameShowType && sameImage;
		}

		private static bool AreLinksEqual(Uri? one, Uri? two)
		{
			if (one is null && two is null)
			{
				return true;
			}

			if ((one is null) != (two is null))
			{
				return false;
			}

			if ((one is not null) && (two is not null))
			{
				return one.Equals(two);
			}

			return false;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(base.ToString());
			sb.AppendLine(Title);
			sb.AppendLine(Date.ToString(CultureInfo.CurrentCulture));
			sb.AppendLine(IsPremium ? "premium" : "not premium");
			sb.AppendLine(Type);
			sb.AppendLine(Image?.AbsoluteUri ?? "no image uri");

			return sb.ToString();
		}
	}
}
