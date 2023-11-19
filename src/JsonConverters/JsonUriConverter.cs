using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GBLive.JsonConverters
{
	public class JsonUriConverter : JsonConverter<Uri>
	{
		private const string https = "https://";

		public override Uri Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			string? raw = reader.GetString();

			if (String.IsNullOrWhiteSpace(raw))
			{
				throw new ArgumentException(nameof(raw));
			}

			if (!raw.StartsWith(https, StringComparison.OrdinalIgnoreCase))
			{
				raw = raw.Insert(0, https);
			}

			return new Uri(raw, UriKind.Absolute);
		}

		public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options)
		{
			ArgumentNullException.ThrowIfNull(writer);
			ArgumentNullException.ThrowIfNull(value);

			writer.WriteString("image", value.AbsoluteUri);
		}
	}
}
