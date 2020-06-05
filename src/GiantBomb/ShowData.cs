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

            sb.AppendLine($"type: {Type}");
            sb.AppendLine($"title: {Title}");
            sb.AppendLine($"image: {Image}");
            sb.AppendLine($"date: {Date.ToString()}");
            sb.AppendLine($"is premium: {Premium}");

            return sb.ToString();
        }
    }
}
