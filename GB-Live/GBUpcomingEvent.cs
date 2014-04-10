﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Data;

namespace GB_Live
{
    public enum GBEventType { Unknown, Article, Podcast, Video, LiveShow };

    public class GBUpcomingEvent : IComparable<GBUpcomingEvent>, IEquatable<GBUpcomingEvent>
    {
        public string Title { get; private set; }
        public DateTime Time { get; private set; }
        public GBEventType EventType { get; private set; }
        public Uri BackgroundImageUrl { get; private set; }

        public GBUpcomingEvent(string s)
        {
            this.Title = GetTitleFromString(s);
            this.Time = GetTimeFromString(s);
            this.EventType = GetEventTypeFromString(s);
            this.BackgroundImageUrl = GetBackgroundImageUrlFromString(s);
        }

        private string GetTitleFromString(string s)
        {
            int beginning = s.IndexOf("itle\">") + 6;
            int ending = s.IndexOf("<", beginning);
            int length = ending - beginning;

            return HttpUtility.HtmlDecode(s.Substring(beginning, length));
        }

        private DateTime GetTimeFromString(string s)
        {
            int beginning = s.IndexOf("ime\">") + 5;
            int ending = s.IndexOf("<", beginning);
            int length = ending - beginning;

            string stringToParse = HttpUtility.HtmlDecode(s.Substring(beginning, length));

            return TryParseAndTrim(stringToParse, false);
        }

        private GBEventType GetEventTypeFromString(string s)
        {
            int beginning = s.IndexOf("ime\">") + 5;
            int ending = s.IndexOf("<", beginning);
            int length = ending - beginning;

            string actualText = HttpUtility.HtmlDecode(s.Substring(beginning, length));

            if (actualText.StartsWith("Article"))
            {
                return GBEventType.Article;
            }
            else if (actualText.StartsWith("Podcast"))
            {
                return GBEventType.Podcast;
            }
            else if (actualText.StartsWith("Video"))
            {
                return GBEventType.Video;
            }
            else if (actualText.StartsWith("Live Show"))
            {
                return GBEventType.LiveShow;
            }
            else
            {
                return GBEventType.Unknown;
            }
        }

        private Uri GetBackgroundImageUrlFromString(string s)
        {
            int beginning = s.IndexOf("(") + 1;
            int ending = s.IndexOf(")", beginning);
            int length = ending - beginning;

            return new Uri(s.Substring(beginning, length));
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
                    return TryParseAndTrim(TrimStringFromBeginning(s), fromEnd);
                }
            }
        }

        private string TrimStringFromBeginning(string s)
        {
            char[] c = s.ToCharArray(1, s.Length - 1);

            return new string(c);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("Title: {0}", this.Title));
            sb.AppendLine(string.Format("Time: {0}", this.Time));
            sb.AppendLine(string.Format("Event type: {0}", this.EventType.ToString()));
            sb.AppendLine(string.Format("Image url: {0}", this.BackgroundImageUrl.AbsoluteUri));

            return sb.ToString();
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
            if (this.Title.Equals(other.Title) && this.Time.Equals(other.Time))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class AddSpacesToEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            GBEventType eventType = (GBEventType)value;
            string eventTypeAsString = eventType.ToString();

            if (StringHasExtraUppercase(eventTypeAsString))
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

        private bool StringHasExtraUppercase(string s)
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
}
