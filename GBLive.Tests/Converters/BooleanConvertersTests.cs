using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GBLive.WPF.Converters;
using NUnit.Framework;

namespace GBLive.Tests.Converters
{
    [TestFixture]
    public class BooleanConvertersTests
    {
        [TestCase(true, "fish", "pond")]
        [TestCase(false, "sheep", "pen")]
        public void BooleanToStringConverter_Convert(bool value, string trueString, string falseString)
        {
            var converter = new BooleanToStringConverter
            {
                True = trueString,
                False = falseString
            };

            var result = converter.Convert(value, typeof(object), null, CultureInfo.InvariantCulture);

            Assert.IsInstanceOf<string>(result);

            string expected = value ? trueString : falseString;
            string actual = (string)result;

            Assert.AreEqual(expected, actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BooleanToStyleConverter_Convert(bool value)
        {
            Style trueStyle = new Style(typeof(Label));
            trueStyle.Setters.Add(new Setter(Control.FontSizeProperty, 22d));

            Style falseStyle = new Style(typeof(Label));
            falseStyle.Setters.Add(new Setter(Control.FontFamilyProperty, new FontFamily("Calibri")));

            var converter = new BooleanToStyleConverter
            {
                True = trueStyle,
                False = falseStyle
            };

            var result = converter.Convert(value, typeof(object), null, CultureInfo.InvariantCulture);

            Assert.IsInstanceOf<Style>(result);

            Style expected = value ? trueStyle : falseStyle;
            Style actual = (Style)result;

            Assert.AreEqual(expected, actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BooleanToBrushConverter_Convert(bool value)
        {
            var trueBrush = Brushes.Gold;
            var falseBrush = Brushes.Red;

            var converter = new BooleanToBrushConverter
            {
                True = trueBrush,
                False = falseBrush
            };

            var result = converter.Convert(value, typeof(object), null, CultureInfo.InvariantCulture);

            Assert.IsInstanceOf<Brush>(result);

            Brush expected = value ? trueBrush : falseBrush;
            Brush actual = (Brush)result;

            Assert.AreEqual(expected, actual);
        }
    }
}
