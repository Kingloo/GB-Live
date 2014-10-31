using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GB_Live.Tests
{
    [TestClass]
    public class GBUpcomingEventTests
    {
        private static string goodHtml = "<dl class=\"promo-upcoming\"><dt>Coming up on Giant Bomb</dt><dd style=\"background-image:url(http://static.giantbomb.com/uploads/screen_medium/0/31/2695491-cp_dreamfall_chapters.jpg)\"  class=\"\"><div><h4 class=\"title\">Quick Look: Dreamfall Chapters: Book One: Reborn</h4><p class=\"time\">Video on Oct 27, 2014 06:00 PM</p></div></dd><dd style=\"background-image:url(http://static.giantbomb.com/uploads/screen_medium/0/31/2695498-uf_hyperlightdrifter_102414.jpg)\"  class=\"\"><div><h4 class=\"title\">Unfinished: Hyper Light Drifter 10/24/2014</h4><p class=\"time\">Video on Oct 28, 2014 06:00 PM</p></div></dd><dd style=\"background-image:url(http://static.giantbomb.com/uploads/screen_medium/9/93998/2694702-9817449206-Image.jpg)\"  class=\"content--premium\"><div><h4 class=\"title\">Spookin&#039; With Scoops: Get Spooked Boi!</h4><p class=\"time\">Video on Oct 31, 2014 07:00 AM</p></div></dd><dd style=\"background-image:url(http://static.giantbomb.com/uploads/screen_medium/2/24459/2238401-e3__photobooth_3.jpg)\"  class=\"content--premium\"><div><h4 class=\"title\">You Can&#039;t Go (PlayStation) Home Again</h4><p class=\"time\">Live Show on Mar 31, 2015 09:00 PM</p></div></dd></dl>";
        private static string badHtml = string.Empty;

        [TestMethod]
        public void TryCreate_EmptyHtml()
        {
            GBUpcomingEvent test = null;

            if (GBUpcomingEvent.TryCreate(string.Empty, out test))
            {
                Assert.IsNotNull(test);
            }
            else
            {
                Assert.IsNull(test);
            }
        }

        [TestMethod]
        public void TryCreate_goodHtml()
        {
            GBUpcomingEvent test = null;

            if (GBUpcomingEvent.TryCreate(goodHtml, out test))
            {
                Assert.IsNotNull(test);
            }
            else
            {
                Assert.IsNull(test);
            }
        }
    }
}
