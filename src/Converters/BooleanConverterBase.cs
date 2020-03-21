using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GBLive.Converters
{
    public abstract class BooleanConverterBase<T> : DependencyObject, IValueConverter
    {
        [property:NotNull]
        public T True
        {
            get => (T)GetValue(TrueProperty);
            set => SetValue(TrueProperty, value);
        }

        [property: NotNull]
        public T False
        {
            get => (T)GetValue(FalseProperty);
            set => SetValue(FalseProperty, value);
        }

        public static readonly DependencyProperty TrueProperty = DependencyProperty.Register(
            "True",
            typeof(T),
            typeof(BooleanConverterBase<T>),
            new PropertyMetadata(null));

        public static readonly DependencyProperty FalseProperty = DependencyProperty.Register(
            "False",
            typeof(T),
            typeof(BooleanConverterBase<T>),
            new PropertyMetadata(null));

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? True : False;
            }
            else
            {
                throw new NullReferenceException($"bool converter from {targetType.FullName} was passed a null value");
            }
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException($"converting from {value.GetType().ToString()} to {targetType.ToString()} is not implemented!");
    }
}
