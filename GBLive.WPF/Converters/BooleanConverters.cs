using System.Windows;
using System.Windows.Media;

namespace GBLive.WPF.Converters
{
    public class BooleanToStringConverter : BooleanConverterBase<string> { }

    public class BooleanToStyleConverter : BooleanConverterBase<Style> { }

    public class BooleanToBrushConverter : BooleanConverterBase<Brush> { }
}
