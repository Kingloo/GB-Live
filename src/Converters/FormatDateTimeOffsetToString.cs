using System;
using System.Globalization;
using System.Windows.Data;

namespace GBLive.Converters
{
	public class FormatDateTimeOffsetToString : IValueConverter
	{
		private const string defaultFormat = "ddd MMM dd";

		public string ErrorMessage { get; set; } = string.Empty;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string format = (parameter is string providedFormat) ? providedFormat : defaultFormat;

			return value switch
			{
				DateTimeOffset dto => dto.ToString(format, culture),
				_ => ErrorMessage
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> DateTimeOffset.TryParse((string)value, out DateTimeOffset result) ? result : DateTimeOffset.MaxValue;
	}
}
