using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GBLive.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToStringConverter : BooleanConverterBase<string> { }

    [ValueConversion(typeof(bool), typeof(Style))]
    public class BooleanToStyleConverter : BooleanConverterBase<Style> { }

    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BooleanToBrushConverter : BooleanConverterBase<Brush> { }
}