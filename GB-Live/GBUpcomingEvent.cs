using System;
using System.Text;
using System.Web;

namespace GB_Live
{
    public enum GBEventType { Unknown, Article, Review, Podcast, Video, LiveShow };

    public class GBUpcomingEvent : ViewModelBase, IComparable<GBUpcomingEvent>, IEquatable<GBUpcomingEvent>
    {
        #region Fields
        private CountdownDispatcherTimer countdown = null;
        private readonly DateTime creationDate = DateTime.Now;
        #endregion

        #region Properties
        public string Title { get; private set; }
        private DateTime _time = DateTime.MinValue;
        public DateTime Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;

                OnNotifyPropertyChanged();
            }
        }
        public bool Premium { get; private set; }
        public GBEventType EventType { get; private set; }
        public Uri BackgroundImageUrl { get; private set; }
        #endregion

        public GBUpcomingEvent() { }

        public GBUpcomingEvent(string title, DateTime time, bool premium, GBEventType type)
        {
            this.Title = title;
            this.Time = time;
            this.Premium = premium;
            this.EventType = type;
        }

        private GBUpcomingEvent(string s)
        {
            string f = HttpUtility.HtmlDecode(s);

            this.Title = GetTitle(f);
            this.Premium = GetPremium(f);
            this.BackgroundImageUrl = GetBackgroundImageUrl(f);

            if (Premium)
            {
                string beginning = "</span>";
                string ending = "</p>";

                SetTimeAndEventType(f, beginning, ending);
            }
            else
            {
                string beginning = "ime\">";
                string ending = "</p>";

                SetTimeAndEventType(f, beginning, ending);
            }
        }

        public static bool TryCreate(string html, out GBUpcomingEvent outEvent)
        {
            if (String.IsNullOrEmpty(html)
                || String.IsNullOrWhiteSpace(html)
                || html.Contains("<") == false
                || html.Contains(">") == false
                || html.Contains("title") == false
                || html.Contains("time") == false)
            {
                outEvent = null;

                return false;
            }

            outEvent = new GBUpcomingEvent(html);

            return true;
        }

        private string GetTitle(string s)
        {
            string beginning = "itle\">";
            string ending = "<";

            FromBetweenResult res = s.FromBetween(beginning, ending);

            if (res.Result == Result.Success)
            {
                return res.ResultString;
            }
            else
            {
                return "Title Unknown";
            }
        }

        private bool GetPremium(string s)
        {
            return s.Contains("content--premium");
        }

        private Uri GetBackgroundImageUrl(string s)
        {
            string beginning = "background-image:url(";
            string ending = ")";

            FromBetweenResult res = s.FromBetween(beginning, ending);

            if (res.Result == Result.Success)
            {
                string raw = res.ResultString;

                Uri uri = null;
                if (Uri.TryCreate(raw, UriKind.Absolute, out uri))
                {
                    return uri; // desired scenario
                }
            }

            // fallback
            return new UriBuilder
            {
                Scheme = "http",
                Host = "static.giantbomb.com",
                Path = "/bundles/phoenixsite/images/core/loose/apple-touch-icon-precomposed-gb.png",
                Port = 80,
            }
            .Uri;
        }

        private void SetTimeAndEventType(string whole, string beginning, string ending)
        {
            FromBetweenResult res = whole.FromBetween(beginning, ending);

            if (res.Result == Result.Success)
            {
                string raw = res.ResultString;
                string between = raw.Trim();

                Time = GetTime(between);
                EventType = GetEventType(between);
            }
            else
            {
                Utils.LogMessage(string.Format("{0} occurred while looking between {1} and {2}", res.Result.ToString(), beginning, ending));
            }
        }

        private DateTime GetTime(string s)
        {
            return TryParseAndTrim(s, false);
        }

        private GBEventType GetEventType(string s)
        {
            if (s.StartsWith("Article", StringComparison.InvariantCultureIgnoreCase))
            {
                return GBEventType.Article;
            }
            else if (s.StartsWith("Review", StringComparison.InvariantCultureIgnoreCase))
            {
                return GBEventType.Review;
            }
            else if (s.StartsWith("Podcast", StringComparison.InvariantCultureIgnoreCase))
            {
                return GBEventType.Podcast;
            }
            else if (s.StartsWith("Video", StringComparison.InvariantCultureIgnoreCase))
            {
                return GBEventType.Video;
            }
            else if (s.StartsWith("Live Show", StringComparison.InvariantCultureIgnoreCase))
            {
                return GBEventType.LiveShow;
            }
            else
            {
                return GBEventType.Unknown;
            }
        }

        public void StartCountdownTimer()
        {
            Int64 ticks = CalculateTicks();

            if (CanStartCountdownWithTicks(ticks))
            {
                countdown = new CountdownDispatcherTimer(new TimeSpan(ticks), countdown_Fire);
            }
        }

        private void countdown_Fire()
        {
            Time = DateTime.MaxValue;

            NotificationService.Send(Title, Globals.gbHome);
        }

        private Int64 CalculateTicks()
        {
            TimeSpan fromNowToEvent = Time - DateTime.Now;
            TimeSpan oneMinuteInTicks = new TimeSpan(600000000L); // 10,000 ticks in 1 ms => 10,000 * 1000 ticks in 1 s => 10,000 * 1000 * 60 ticks in 1 min == 600,000,000 ticks

            TimeSpan addedOneMinute = fromNowToEvent.Add(oneMinuteInTicks);

            Int64 ticksUntilEvent = addedOneMinute.Ticks;

#if DEBUG
            return fromNowToEvent.Ticks;
#else
            return ticksUntilEvent;
#endif
        }

        private bool CanStartCountdownWithTicks(Int64 ticks)
        {
            if (ticks <= 0L) return false;

            /*
            * even though you can start a DispatcherTimer with a ticks type of Int64,
            * the equivalent number of milliseconds cannot exceed Int32.MaxValue
            * 
            * http://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Threading/DispatcherTimer.cs
            * -> ctor with TimeSpan, DispatcherPriority, EventHandler, Dispatcher
            */

            Int64 millisecondsUntilEvent = ticks / 10000;

            if (millisecondsUntilEvent > Convert.ToInt64(Int32.MaxValue))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void StopCountdownTimer()
        {
            if (countdown.IsActive)
            {
                countdown.Stop();
            }
        }

        private DateTime TryParseAndTrim(string s, bool fromEnd)
        {
            DateTime dt = DateTime.MinValue;

            if (DateTime.TryParse(s, out dt))
            {
                return dt;
            }
            else
            {
                if ((s.Length - 1) <= 0)
                {
                    return DateTime.MinValue;
                }

                if (fromEnd)
                {
                    return TryParseAndTrim(s.Substring(0, s.Length - 1), fromEnd);
                }
                else
                {
                    return TryParseAndTrim(s.Substring(1, s.Length - 1), fromEnd);
                }
            }
        }

        public int CompareTo(GBUpcomingEvent other)
        {
            if (this.Time > other.Time)
            {
                return 1;
            }
            else if (this.Time < other.Time)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        public bool Equals(GBUpcomingEvent other)
        {
            if (this.Title.Equals(other.Title) == false)
            {
                return false;
            }

            if (this.Time.Equals(other.Time) == false)
            {
                return false;
            }

            if (this.Premium.Equals(other.Premium) == false)
            {
                return false;
            }

            if (this.EventType.Equals(other.EventType) == false)
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("Title: {0}", Title));
            sb.AppendLine(string.Format("Time: {0}", Time));
            sb.AppendLine(string.Format("Created: {0}", creationDate.ToString()));
            sb.AppendLine("Countdown:");
            sb.Append(countdown.ToString());
            sb.AppendLine(string.Format("Event type: {0}", EventType.ToString()));
            sb.AppendLine(string.Format("Premium: {0}", Premium.ToString()));
            sb.AppendLine(string.Format("Image url: {0}", BackgroundImageUrl.AbsoluteUri));

            return sb.ToString();
        }
    }
}
