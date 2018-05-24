using System;
using System.Globalization;
using GBLive.WPF.Converters;
using NUnit.Framework;

namespace GBLive.Tests.Converters
{
    [TestFixture]
    public class FormatDateTimeOffsetToStringTests
    {
        private FormatDateTimeOffsetToString converter = new FormatDateTimeOffsetToString();
        private DateTimeOffset time = new DateTimeOffset(2018, 5, 22, 13, 14, 15, TimeSpan.Zero);

        [Test]
        public void Convert_ValidDateTimeOffset_ReturnsCorrectlyFormattedString()
        {
            var result = converter.Convert(time, typeof(object), null, CultureInfo.CurrentCulture);

            Assert.IsInstanceOf<string>(result);

            string expected = "Tue May 22"; // matches with "private DateTimeOffset time..."
            string actual = (string)result;

            Assert.AreEqual(expected, actual);
        }

        [TestCase("dd MM d", "22 05 22")]
        [TestCase("", "22/05/2018 13:14:15 +00:00")]
        public void Convert_ValidDateTimeOffsetWithParameter_ReturnsFormattedByParameter(string parameter, string expected)
        {
            var result = converter.Convert(time, typeof(object), parameter, CultureInfo.CurrentCulture);

            Assert.IsInstanceOf<string>(result);

            string actual = (string)result;

            Assert.AreEqual(expected, actual);
        }

        [TestCase("an error message")]
        [TestCase("a different error message")]
        public void Convert_NotADateTimeOffset_ReturnsErrorMessage(string errorMessage)
        {
            var myConverter = new FormatDateTimeOffsetToString
            {
                ErrorMessage = errorMessage
            };

            var result = myConverter.Convert(null, typeof(object), string.Empty, CultureInfo.CurrentCulture);

            Assert.IsInstanceOf<string>(result);

            string expected = errorMessage;
            string actual = (string)result;

            Assert.AreEqual(expected, actual);
        }
    }
}
