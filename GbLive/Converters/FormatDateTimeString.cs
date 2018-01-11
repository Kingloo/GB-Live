using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace GbLive.Converters
{
    public class FormatDateTimeString : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime time)
            {
                string format = "ddd MMM dd  -  HH:mm";

                return time.ToString(format, culture);
            }
            else
            {
                return "Time Conversion Error!";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DateTime.MaxValue;

        public override object ProvideValue(IServiceProvider serviceProvider)
            => this;
    }
}
