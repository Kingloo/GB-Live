using System.Text;

namespace GBLive.GiantBomb
{
    public class LiveNowData
    {
        public string Title { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;

        public LiveNowData() { }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("live now");

            sb.AppendLine($"title: {Title}");
            sb.AppendLine($"image: {Image}");

            return sb.ToString();
        }
    }
}
