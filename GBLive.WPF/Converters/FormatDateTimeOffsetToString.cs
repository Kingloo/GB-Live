using System;
using System.Globalization;
using System.Windows.Data;

namespace GBLive.WPF.Converters
{
    public class FormatDateTimeOffsetToString : IValueConverter
    {
        public string ErrorMessage { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTimeOffset dt)
            {
                string format = (parameter is string providedFormat) ? providedFormat : "ddd MMM dd";

                return dt.ToString(format, culture);
            }
            else
            {
                return ErrorMessage;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DateTimeOffset.TryParse((string)value, out DateTimeOffset result) ? result : DateTimeOffset.MaxValue;
    }
}
