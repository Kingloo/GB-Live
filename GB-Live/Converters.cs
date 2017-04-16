using System;
using System.Configuration;
using System.Globalization;
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
            => (bool)value ? True : False;

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => default(T);
    }

    [ValueConversion(typeof(bool), typeof(Style))]
    public class BooleanToStyleConverter : GenericBooleanConverter<Style> { }

    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BooleanToBrushConverter : GenericBooleanConverter<Brush> { }
    
    
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToLiveStatusStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = (bool)value;

            return b
                ? ConfigurationManager.AppSettings["GBIsLiveMessage"]
                : ConfigurationManager.AppSettings["GBIsNotLiveMessage"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => null;
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
            // do NOT change this to 'return null;'
            // breaks!!

            return this;
        }
    }
}
