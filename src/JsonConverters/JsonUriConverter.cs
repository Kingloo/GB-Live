using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GBLive.GiantBomb;

namespace GBLive.JsonConverters
{
	public class JsonUriConverter : JsonConverter<Uri>
	{
		private const string https = "https://";

		public override Uri Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.GetString() is string value
				&& value.Length > 0)
			{
				if (!value.StartsWith(https, StringComparison.OrdinalIgnoreCase))
				{
					value = value.Insert(0, https);
				}

				return new Uri(value, UriKind.Absolute);
			}
			else
			{
				return Constants.FallbackImage;
			}
		}

		public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options)
		{
			ArgumentNullException.ThrowIfNull(writer);
			ArgumentNullException.ThrowIfNull(value);

			writer.WriteString("image", value.AbsoluteUri);
		}
	}
}
