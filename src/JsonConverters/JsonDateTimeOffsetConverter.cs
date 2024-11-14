using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GBLive.JsonConverters
{
	internal sealed class JsonDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
	{
		// Dec 20, 2022 11:00 AM
		// hh (lowercase h) is for 12-hour clocks
		private const string giantbombDateTimeFormat = "MMM dd, yyyy hh:mm tt";

		// IANA: America/Los_Angeles
		private const string giantbombTimeZoneWindowsName = "Pacific Standard Time";

		public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			DateTimeOffset giantbombTime = DateTimeOffset.ParseExact(reader.GetString()!, giantbombDateTimeFormat, CultureInfo.InvariantCulture);

			TimeSpan localOffset = TimeZoneInfo.Local.GetUtcOffset(DateTimeOffset.Now);

			TimeSpan pacificOffset = TimeZoneInfo.FindSystemTimeZoneById(giantbombTimeZoneWindowsName).GetUtcOffset(DateTimeOffset.Now);

			TimeSpan relativeOffset = localOffset - pacificOffset;

			return giantbombTime.Add(relativeOffset);
		}

		public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
		{
			ArgumentNullException.ThrowIfNull(writer);

			writer.WriteStringValue(value.ToUnixTimeSeconds().ToString(CultureInfo.CurrentCulture));
		}
	}
}
