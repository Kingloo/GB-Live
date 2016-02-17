﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace GB_Live
{
    public abstract class GenericBooleanConverter<T> : IValueConverter
    {
        public T True { get; set; }
        public T False { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = (bool)value;

            if (b)
            {
                return True;
            }
            else
            {
                return False;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default(T);
        }
    }

    [ValueConversion(typeof(bool), typeof(Style))]
    public class BooleanToLabelStyleConverter : GenericBooleanConverter<Style> { }

    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BooleanToColourConverter : GenericBooleanConverter<Brush> { }


    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToLiveStatusStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = (bool)value;

            if (b)
            {
                return ConfigurationManager.AppSettings["GBIsLiveMessage"];
            }
            else
            {
                return ConfigurationManager.AppSettings["GBIsNotLiveMessage"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Enum), typeof(string))]
    public class AddSpacesToEnumConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

        private static bool StringHasExtraUppercaseCharacters(string s)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    [ValueConversion(typeof(DateTime), typeof(string))]
    public class FormatDateTimeString : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dt = (DateTime)value;

            if (dt.Equals(DateTime.MaxValue))
            {
                return "Now!";
            }
            else
            {
                return dt.ToString("ddd MMM dd  -  HH:mm", CultureInfo.CurrentCulture);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DateTime.MaxValue;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
