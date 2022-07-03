using System.Text;

namespace GBLive.GiantBomb
{
	public class ShowData
	{
		public string Type { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string Image { get; set; } = string.Empty;
		public string Date { get; set; } = string.Empty;
		public bool Premium { get; set; } = false;

		public ShowData() { }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("show");

#pragma warning disable CA1305
			sb.AppendLine($"type: {Type}");
			sb.AppendLine($"title: {Title}");
			sb.AppendLine($"image: {Image}");
			sb.AppendLine($"date: {Date.ToString()}");
			sb.AppendLine($"is premium: {Premium}");
#pragma warning restore CA1305

			return sb.ToString();
		}
	}
}
