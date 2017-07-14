using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace GBLive.Desktop.Common
{
    public abstract class GenericBooleanConverter<T> : DependencyObject, IValueConverter
    {
        public T True
        {
            get { return (T)GetValue(TrueProperty); }
            set { SetValue(TrueProperty, value); }
        }
        
        public static readonly DependencyProperty TrueProperty =
            DependencyProperty.Register("True", typeof(T), typeof(GenericBooleanConverter<T>), new PropertyMetadata(default(T)));
        
        public T False
        {
            get { return (T)GetValue(FalseProperty); }
            set { SetValue(FalseProperty, value); }
        }
        
        public static readonly DependencyProperty FalseProperty =
            DependencyProperty.Register("False", typeof(T), typeof(GenericBooleanConverter<T>), new PropertyMetadata(default(T)));

        
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
    public class BooleanToLiveStatusConverter : GenericBooleanConverter<string> { }

    
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class FormatDateTimeString : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime time = (DateTime)value;

            string format = "ddd MMM dd  -  HH:mm";

            return time.Equals(DateTime.MaxValue) ? "Now!" : time.ToString(format, culture);
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
