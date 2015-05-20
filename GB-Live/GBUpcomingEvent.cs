using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows;
using System.Windows.Threading;

namespace GB_Live
{
    public enum GBEventType { Unknown, Article, Review, Podcast, Video, LiveShow };

    public class GBUpcomingEvent : ViewModelBase, IComparable<GBUpcomingEvent>, IEquatable<GBUpcomingEvent>
    {
        #region Fields
        //private DispatcherTimer countdownTimer = null;
        #endregion

        #region Properties
        public string Title { get; private set; }
        private DateTime _time = DateTime.MinValue;
        public DateTime Time
        {
            get
            {
                return this._time;
            }
            set
            {
                this._time = value;

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
            this.Title = GetTitle(s);
            this.Premium = GetPremium(s);
            this.BackgroundImageUrl = GetBackgroundImageUrl(s);

            if (Premium)
            {
                string raw = s.FromBetween("</span>", "</p>");
                string between = raw.Trim();

                this.Time = GetTime(between);
                this.EventType = GetEventType(between);
            }
            else
            {
                string raw = s.FromBetween("ime\">", "</p>");
                string between = raw.Trim();

                this.Time = GetTime(between);
                this.EventType = GetEventType(between);
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
            int beginning = s.IndexOf("itle\">") + 6; // + 6 to move past the > and into the actual content
            int ending = s.IndexOf("<", beginning);
            int length = ending - beginning;

            return HttpUtility.HtmlDecode(s.Substring(beginning, length));
        }

        private bool GetPremium(string s)
        {
            if (s.Contains("content--premium"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Uri GetBackgroundImageUrl(string s)
        {
            string raw = s.FromBetween("background-image:url(", ")");
            Uri uri = null;

            if (Uri.TryCreate(raw, UriKind.Absolute, out uri))
            {
                return uri;
            }
            else
            {
                UriBuilder ub = new UriBuilder
                {
                    Host = "static.giantbomb.com",
                    Path = "/bundles/phoenixsite/images/core/loose/apple-touch-icon-precomposed-gb.png",
                    Port = 80,
                    Scheme = "http"
                };

                return ub.Uri;
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

            if (CanStartDispatcherTimerWithTicks(ticks))
            {
                //countdownTimer = new DispatcherTimer
                //{
                //    Interval = new TimeSpan(ticks)
                //    //Interval = new TimeSpan(0, 0, 25)
                //};

                //countdownTimer.Tick += countdownTimer_Tick;
                //countdownTimer.Start();

                CountdownDispatcherTimer timer = new CountdownDispatcherTimer(new TimeSpan(ticks), () =>
                    {
                        Time = DateTime.MaxValue;

                        NotificationService.Send(Title, Globals.gbHome);
                    });
            }
        }

        private Int64 CalculateTicks()
        {
            TimeSpan fromNowToEvent = this.Time - DateTime.Now;
            TimeSpan oneMinuteInTicks = new TimeSpan(600000000L); // 10,000 ticks in 1 ms => 10,000 * 1000 ticks in 1 s => 10,000 * 1000 * 60 ticks in 1 min == 600,000,000 ticks

            TimeSpan addedOneMinute = fromNowToEvent.Add(oneMinuteInTicks);

            Int64 ticksUntilEvent = addedOneMinute.Ticks;

            return ticksUntilEvent;
            //return fromNowToEvent.Ticks;
        }

        private bool CanStartDispatcherTimerWithTicks(Int64 ticks)
        {
            if (ticks == 0L) return false;

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

            return true;
        }

        //public void StopCountdownTimer()
        //{
        //    if (countdownTimer != null)
        //    {
        //        countdownTimer.Stop();
        //        countdownTimer.Tick -= countdownTimer_Tick;
        //        countdownTimer = null;
        //    }
        //}

        //private void countdownTimer_Tick(object sender, EventArgs e)
        //{
        //    this.Time = DateTime.MaxValue;

        //    StopCountdownTimer();

        //    NotificationService.Send(this.Title, Globals.gbHome);
        //}

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

            sb.AppendLine(string.Format("Title: {0}", this.Title));
            sb.AppendLine(string.Format("Time: {0}", this.Time));

            //if (this.countdownTimer != null)
            //{
            //    sb.AppendLine(string.Format("Countdown timer enabled: {0}", this.countdownTimer.IsEnabled));
            //}
            //else
            //{
            //    sb.AppendLine("Countdown timer not created");
            //}

            sb.AppendLine(string.Format("Event type: {0}", this.EventType.ToString()));
            sb.AppendLine(string.Format("Premium: {0}", this.Premium.ToString()));
            sb.AppendLine(string.Format("Image url: {0}", this.BackgroundImageUrl.AbsoluteUri));

            return sb.ToString();
        }
    }
}
