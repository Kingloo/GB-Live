using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace GBLive.GiantBomb
{
    public class UpcomingData
    {
        public LiveNowData? LiveNow { get; set; }
        public IReadOnlyCollection<ShowData> Upcoming { get; set; } = new Collection<ShowData>();

        public UpcomingData() { }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(LiveNow?.ToString() ?? "no live show");

            if (Upcoming.Count > 0)
            {
                foreach (ShowData each in Upcoming)
                {
                    sb.AppendLine(each.ToString());
                }
            }
            else
            {
                sb.AppendLine("no shows");
            }

            return sb.ToString();
        }
    }
}
