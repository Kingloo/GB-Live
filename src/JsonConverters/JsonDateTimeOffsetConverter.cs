using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GBLive.JsonConverters
{
	public class JsonDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
	{
		// Dec 20, 2022 11:00 AM
		private const string giantbombDateTimeFormat = "MMM dd, yyyy HH:mm tt";

		public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			DateTimeOffset giantbombTime = DateTimeOffset.ParseExact(reader.GetString()!, giantbombDateTimeFormat, CultureInfo.InvariantCulture);

			TimeSpan local = TimeZoneInfo.Local.GetUtcOffset(DateTimeOffset.Now);
			TimeSpan pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time").GetUtcOffset(DateTimeOffset.Now);

			TimeSpan offset = local - pacific;
			
			return giantbombTime.Add(offset);
		}

		public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
		{
			if (writer is null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			writer.WriteStringValue(value.ToUnixTimeSeconds().ToString(CultureInfo.CurrentCulture));
		}
	}
}