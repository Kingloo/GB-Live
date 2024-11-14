using System.Globalization;
using System.Text;

namespace GBLive.GiantBomb
{
	internal sealed class LiveNowData
	{
		public string Title { get; set; } = string.Empty;
		public string Image { get; set; } = string.Empty;

		public LiveNowData() { }

		public override string ToString()
		{
			return new StringBuilder()
				.AppendLine("live now")
				.AppendLine(CultureInfo.InvariantCulture, $"title: {Title}")
				.AppendLine(CultureInfo.InvariantCulture, $"image: {Image}")
				.ToString();
		}
	}
}
