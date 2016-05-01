using System;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Web;
using System.Windows.Threading;
using GB_Live.Extensions;
using Newtonsoft.Json.Linq;

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
        
        public static bool TryCreateFromJson(JObject token, out GBUpcomingEvent outEvent)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
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

            string title = (string)token["title"];
            DateTime time = GetTimeFromJson(token);
            bool premium = (bool)token["premium"];
            GBEventType type = GetEventTypeFromJson(token);
            Uri imageUri = GetBackgroundImageUrlFromJson(token);

            outEvent = new GBUpcomingEvent(title, time, premium, type, imageUri);

            return true;
        }

        private static Uri GetBackgroundImageUrlFromJson(JObject token)
        {
            string uri = HttpUtility.HtmlDecode((string)token["image"]);

            if (String.IsNullOrWhiteSpace(uri))
            {
                return new UriBuilder
                {
                    Scheme = "http",
                    Host = "static.giantbomb.com",
                    Path = "/bundles/phoenixsite/images/core/loose/apple-touch-icon-precomposed-gb.png",
                    Port = 80,
                }
                .Uri;
            }
            else
            {
                return new Uri(uri);
            }
        }

        private static DateTime GetTimeFromJson(JToken token)
        {
<<<<<<< HEAD
            string timeWithoutMeridiem = time.Remove(time.Length - 3);

            string timeWithTZ = AddTimeZoneData(timeWithoutMeridiem);
=======
            string time = (string)token["date"];
>>>>>>> parent of 60fc417... Correct timezones

            DateTime dt = DateTime.MinValue;

            if (DateTime.TryParse(time, out dt))
            {
                string raw = res.ResultValue;

                Uri uri = null;
                if (Uri.TryCreate(raw, UriKind.Absolute, out uri))
                {
                    return uri;
                }
            }
            
            return new UriBuilder
            {
                Scheme = "http",
                Host = "static.giantbomb.com",
                Path = "/bundles/phoenixsite/images/core/loose/apple-touch-icon-precomposed-gb.png",
                Port = 80,
            }
            .Uri;
        }

        private static GBEventType GetEventTypeFromJson(JToken token)
        {
            FromBetweenResult res = whole.FromBetween(beginning, ending);

            if (res.Result == Result.Success)
            {
                string raw = res.ResultValue;
                string between = raw.Trim();

                Time = GetTime(between);
                EventType = GetEventType(between);
            }
            else
            {
                Utils.LogMessage(string.Format(CultureInfo.CurrentCulture, "{0} occurred while looking between {1} and {2}", res.Result.ToString(), beginning, ending));
            }
        }

        private DateTime GetTime(string s)
        {
            return TryParseAndTrim(s, false);
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

            NotificationService.Send(Title, () =>
                {
                    Utils.OpenUriInBrowser(ConfigurationManager.AppSettings["GBHomepage"]);
                });
        }

        private Int64 CalculateTicks()
        {
            TimeSpan fromNowToEvent = Time - DateTime.Now;

            // 10,000 ticks in 1 ms => 10,000 * 1000 ticks in 1 s => 10,000 * 1000 * 20 ticks in 20 secs == 200,000,000 ticks
            TimeSpan twentySecondsInTicks = new TimeSpan(10000 * 1000 * 20);

            TimeSpan addedExtraTime = fromNowToEvent.Add(twentySecondsInTicks);

            Int64 ticksUntilEvent = addedExtraTime.Ticks;

#if DEBUG
            return fromNowToEvent.Ticks;
#else
            return ticksUntilEvent;
#endif
        }

        private static bool CanStartCountdownWithTicks(Int64 ticks)
        {
            if (ticks <= 0L) { return false; }

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
            if (other == null) throw new ArgumentNullException(nameof(other));

            if (Time > other.Time)
            {
                return 1;
            }
            else if (Time < other.Time)
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
            if (other == null) return false;

            if (Title.Equals(other.Title) == false) return false;

            if (Time.Equals(other.Time) == false) return false;

            if (Premium.Equals(other.Premium) == false) return false;

            if (EventType.Equals(other.EventType) == false) return false;

            return true;
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Title: {0}", Title));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Time: {0}", Time));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Created: {0}", creationDate.ToString()));
            sb.AppendLine("Countdown:");
            sb.Append(countdown.ToString());
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Event type: {0}", EventType.ToString()));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Premium: {0}", Premium.ToString()));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Image url: {0}", BackgroundImageUrl.AbsoluteUri));

            return sb.ToString();
        }
    }
}
