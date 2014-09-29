using System;
using System.Windows;
using System.Windows.Data;

namespace GB_Live
{
    [ValueConversion(typeof(bool), typeof(Style))]
    public class BooleanToLabelStyleConverter : IValueConverter
    {
        public Style LabelLiveStyle { get; set; }
        public Style LabelOfflineStyle { get; set; }
        
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isLive = (bool)value;

            if (isLive)
            {
                return this.LabelLiveStyle;
            }
            else
            {
                return this.LabelOfflineStyle;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToLiveStatusStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isLive = (bool)value;

            if (isLive)
            {
                return "Giantbomb is LIVE";
            }
            else
            {
                return "Giantbomb is not streaming";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
}
