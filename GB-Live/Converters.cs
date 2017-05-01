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
        
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? True : False;

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => default(T);
    }

    [ValueConversion(typeof(bool), typeof(Style))]
    public class BooleanToStyleConverter : GenericBooleanConverter<Style> { }

    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BooleanToBrushConverter : GenericBooleanConverter<Brush> { }
    
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToLiveStatusConverter : GenericBooleanConverter<string>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = (bool)value;

            if (!ConfigurationManagerWrapper.TryGetString(True, out string onlineMessage))
            {
                string errorMessage = string.Format(culture, "online message key was not found: {0}", True);

                throw new ConfigurationErrorsException(errorMessage);
            }

            if (!ConfigurationManagerWrapper.TryGetString(False, out string offlineMessage))
            {
                string errorMessage = string.Format(culture, "offline message key was not found: {0}", False);

                throw new ConfigurationErrorsException(errorMessage);
            }

            return b ? onlineMessage : offlineMessage;
        }
    }

    
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class FormatDateTimeString : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dt = (DateTime)value;

            string format = "ddd MMM dd  -  HH:mm";

            return dt.Equals(DateTime.MaxValue) ? "Now!" : dt.ToString(format, culture);
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
