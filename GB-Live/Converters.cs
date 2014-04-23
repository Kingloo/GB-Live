using System;
using System.Windows;
using System.Windows.Data;

namespace GB_Live
{
    [ValueConversion(typeof(bool), typeof(Style))]
    class BooleanToLabelStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isLive = (bool)value;

            if (isLive)
            {
                return Application.Current.Resources["LabelLiveStyle"];
            }
            else
            {
                return Application.Current.Resources["LabelOfflineStyle"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    class BooleanToLiveStatusStringConverter : IValueConverter
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
