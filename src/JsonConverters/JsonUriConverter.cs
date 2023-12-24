using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GBLive.JsonConverters
{
	public class JsonUriConverter : JsonConverter<Uri>
	{
		public override Uri? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return reader.GetString() switch
			{
				string { Length: > 0 } value => new Uri(EnsureStartsWithHttps(value), UriKind.Absolute),
				_ => null
			};
		}

		private static string EnsureStartsWithHttps(string uri)
		{
			ArgumentNullException.ThrowIfNull(uri);

			if (uri.StartsWith(Uri.UriSchemeHttps + Uri.SchemeDelimiter, StringComparison.OrdinalIgnoreCase))
			{
				return uri;
			}

			if (uri.StartsWith(Uri.UriSchemeHttp + Uri.SchemeDelimiter, StringComparison.OrdinalIgnoreCase))
			{
				return uri.Insert(4, "s");
			}

			return string.Concat(Uri.UriSchemeHttps, Uri.SchemeDelimiter, uri);
		}

		public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options)
		{
			ArgumentNullException.ThrowIfNull(writer);
			ArgumentNullException.ThrowIfNull(value);

			writer.WriteString("image", value.AbsoluteUri);
		}
	}
}
