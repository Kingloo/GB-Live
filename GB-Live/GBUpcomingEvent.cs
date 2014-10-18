using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace GB_Live
{
    public enum GBEventType { Unknown, Article, Review, Podcast, Video, LiveShow };

    public class GBUpcomingEvent : ViewModelBase, IComparable<GBUpcomingEvent>, IEquatable<GBUpcomingEvent>
    {
        private DispatcherTimer _countdownTimer = null;

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

        public GBUpcomingEvent(string s)
        {
            this.Title = GetTitleFromString(s);
            this.Time = GetTimeFromString(s);
            this.Premium = GetPremiumStatus(s);
            this.BackgroundImageUrl = GetBackgroundImageUrlFromString(s);

            if (Premium)
            {
                this.EventType = GetEventTypeFromString(s.Substring(9));
            }
            else
            {
                this.EventType = GetEventTypeFromString(s);
            }
        }

        private string GetTitleFromString(string s)
        {
            int beginning = s.IndexOf("itle\">") + 6; // + 6 to move past the > and into the actual content
            int ending = s.IndexOf("<", beginning);
            int length = ending - beginning;

            return HttpUtility.HtmlDecode(s.Substring(beginning, length));
        }

        private DateTime GetTimeFromString(string s)
        {
            int beginning = s.IndexOf("ime\">") + 5; // + 5 to move past the > and into the actual content
            int ending = s.IndexOf("<", beginning);
            int length = ending - beginning;

            string stringToParse = HttpUtility.HtmlDecode(s.Substring(beginning, length));

            return TryParseAndTrim(stringToParse, false);
        }

        public void Update()
        {
            if (this._countdownTimer == null)
            {
                Int64 ticks = CalculateTicks();

                if (CanStartDispatcherTimerWithTicks(ticks))
                {
                    StartCountdownTimer(ticks);
                }
            }
        }

        public void StartCountdownTimer()
        {
            Int64 ticks = CalculateTicks();

            if (CanStartDispatcherTimerWithTicks(ticks))
            {
                this._countdownTimer = new DispatcherTimer
                {
                    Interval = new TimeSpan(ticks)
                };

                this._countdownTimer.Tick += _countdownTimer_Tick;
                this._countdownTimer.IsEnabled = true;
            }
        }

        private void StartCountdownTimer(long ticks)
        {
            this._countdownTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(ticks)
            };

            this._countdownTimer.Tick += _countdownTimer_Tick;
            this._countdownTimer.IsEnabled = true;
        }

        private Int64 CalculateTicks()
        {
            TimeSpan fromNowToEvent = this.Time - DateTime.Now;
            TimeSpan oneMinuteInTicks = new TimeSpan(600000000L); // 1 tick == 10,000 milliseconds => 60,000,000 ticks == 1 minute

            TimeSpan addedOneMinute = fromNowToEvent.Add(oneMinuteInTicks);

            Int64 ticksUntilEvent = addedOneMinute.Ticks;

            return ticksUntilEvent;
        }

        private bool CanStartDispatcherTimerWithTicks(Int64 ticks)
        {
            if (ticks == 0L)
            {
                return false;
            }

            /*
            * even though you can start a DispatcherTimer with a ticks type of Int64,
            * the equivalent number of milliseconds cannot exceed Int32.MaxValue
            * http://referencesource.microsoft.com/#WindowsBase/src/Base/System/Windows/Threading/DispatcherTimer.cs -> ctor
            */

            Int64 millisecondsUntilEvent = ticks / 10000;

            if (millisecondsUntilEvent > Convert.ToInt64(Int32.MaxValue))
            {
                return false;
            }

            return true;
        }

        public void StopCountdownTimer()
        {
            if (this._countdownTimer != null)
            {
                this._countdownTimer.IsEnabled = false;
                this._countdownTimer.Tick -= _countdownTimer_Tick;
                this._countdownTimer = null;
            }
        }

        private void _countdownTimer_Tick(object sender, EventArgs e)
        {
            Application.Current.MainWindow.Dispatcher.Invoke(() =>
                {
                    NotificationService.Send(this.Title, App.gbHome);
                }, DispatcherPriority.Background);

            this.Time = DateTime.MaxValue;
        }

        private GBEventType GetEventTypeFromString(string s)
        {
            int beginning = s.IndexOf("ime\">") + 5;
            int ending = s.IndexOf("<", beginning);
            int length = ending - beginning;

            string actualText = HttpUtility.HtmlDecode(s.Substring(beginning, length));

            if (actualText.StartsWith("Article", StringComparison.InvariantCultureIgnoreCase))
            {
                return GBEventType.Article;
            }
            else if (actualText.StartsWith("Review", StringComparison.InvariantCultureIgnoreCase))
            {
                return GBEventType.Review;
            }
            else if (actualText.StartsWith("Podcast", StringComparison.InvariantCultureIgnoreCase))
            {
                return GBEventType.Podcast;
            }
            else if (actualText.StartsWith("Video", StringComparison.InvariantCultureIgnoreCase))
            {
                return GBEventType.Video;
            }
            else if (actualText.StartsWith("Live Show", StringComparison.InvariantCultureIgnoreCase))
            {
                return GBEventType.LiveShow;
            }
            else
            {
                return GBEventType.Unknown;
            }
        }

        private bool GetPremiumStatus(string s)
        {
            int beginning = s.IndexOf("itle\">") + 6; // + 6 to move past the > and into the actual content
            int ending = s.IndexOf("<", beginning);
            int length = ending - beginning;

            string actualText = HttpUtility.HtmlDecode(s.Substring(beginning, length));

            if (actualText.Contains("Premium"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Uri GetBackgroundImageUrlFromString(string s)
        {
            int beginning = s.IndexOf("(") + 1;
            int ending = s.IndexOf(")", beginning);
            int length = ending - beginning;

            if (beginning == 0)
            {
                return null;
            }
            else
            {
                return new Uri(s.Substring(beginning, length));
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

            sb.AppendLine(string.Format("Title: {0}", this.Title));
            sb.AppendLine(string.Format("Time: {0}", this.Time));

            if (this._countdownTimer != null)
            {
                sb.AppendLine(string.Format("Countdown timer enabled: {0}", this._countdownTimer.IsEnabled));
            }
            else
            {
                sb.AppendLine("Countdown timer not created");
            }

            sb.AppendLine(string.Format("Event type: {0}", this.EventType.ToString()));
            sb.AppendLine(string.Format("Image url: {0}", this.BackgroundImageUrl.AbsoluteUri));

            return sb.ToString();
        }
    }

    public class AddSpacesToEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            GBEventType eventType = (GBEventType)value;
            string eventTypeAsString = eventType.ToString();

            if (StringHasExtraUppercaseCharacters(eventTypeAsString))
            {
                List<int> listExtraUppercaseIndices = new List<int>();

                for (int i = 1; i < eventTypeAsString.Length; i++)
                {
                    if (Char.IsUpper(eventTypeAsString[i]))
                    {
                        listExtraUppercaseIndices.Add(i);
                    }
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(eventTypeAsString);

                for (int i = listExtraUppercaseIndices.Count - 1; i >= 0; i--)
                {
                    sb.Insert(listExtraUppercaseIndices[i], " ");
                }

                return sb.ToString();
            }
            else
            {
                return eventTypeAsString;
            }
        }

        private bool StringHasExtraUppercaseCharacters(string s)
        {
            char[] array = s.ToCharArray();

            for (int i = 1; i < array.Length; i++)
            {
                if (Char.IsUpper(array[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class FormatDateTimeString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime dt = (DateTime)value;

            if (dt.Equals(DateTime.MaxValue))
            {
                return "Now!";
            }
            else
            {
                return dt.ToString("ddd MMM dd  -  HH:mm");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DateTime.MaxValue;
        }
    }
}
