using System;
using System.Linq;
using System.Threading.Tasks;
using GBLive.WPF;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace GBLive.Tests
{
    [TestFixture]
    public class GBUpcomingEventTests
    {
        private GBSettings dummySettings = new GBSettings();

        [Test]
        public void GBUpcomingEvent_TryCreate_TokenNullReturnsFalse()
        {
            bool expected = false;
            bool actual = GBUpcomingEvent.TryCreate(null, out GBUpcomingEvent gbu);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GBUpcomingEvent_TryCreate_WhenTokenIsNullOutObjectIsNull()
        {
            bool b = GBUpcomingEvent.TryCreate(null, out GBUpcomingEvent gbu);

            bool expected = true;
            bool actual = gbu == null;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GBUpcomingEvent_TryCreate_TitleSetCorrectly()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_OneEvent.json");

            JObject json = JObject.Parse(testData);
            JToken token = json["upcoming"].First;

            if (GBUpcomingEvent.TryCreate(token, out GBUpcomingEvent gbu))
            {
                string expected = "The Giant Beastcast - Episode 104";
                string actual = gbu.Title;

                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail("creating the event failed");
            }
        }

        [Test]
        public void GBUpcomingEvent_TryCreate_TimeSetCorrectly()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_OneEvent.json");

            JObject json = JObject.Parse(testData);
            JToken token = json["upcoming"].First;

            if (GBUpcomingEvent.TryCreate(token, out GBUpcomingEvent gbu))
            {
                DateTime expected = new DateTime(2027, 5, 19, 12, 0, 0, DateTimeKind.Local);
                DateTime actual = gbu.Time;

                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail("creating the event failed");
            }
        }

        [Test]
        public void GBUpcomingEvent_TryCreate_IsPremiumSetCorrectly()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_OneEvent.json");

            JObject json = JObject.Parse(testData);
            JToken token = json["upcoming"].First;

            if (GBUpcomingEvent.TryCreate(token, out GBUpcomingEvent gbu))
            {
                bool expected = false;
                bool actual = gbu.IsPremium;

                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail("creating the event failed");
            }
        }

        [Test]
        public void GBUpcomingEvent_TryCreate_EventTypeSetCorrectly()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_OneEvent.json");

            JObject json = JObject.Parse(testData);
            JToken token = json["upcoming"].First;

            if (GBUpcomingEvent.TryCreate(token, out GBUpcomingEvent gbu))
            {
                string expected = "Podcast";
                string actual = gbu.EventType;

                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail("creating the event failed");
            }
        }

        [Test]
        public void GBUpcomingEvent_TryCreate_ImageUriSetCorrectly()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_OneEvent.json");

            JObject json = JObject.Parse(testData);
            JToken token = json["upcoming"].First;

            if (GBUpcomingEvent.TryCreate(token, out GBUpcomingEvent gbu))
            {
                Uri expected = new Uri("https://static.giantbomb.com/uploads/original/0/31/2939712-2369865116-maxre.jpg");
                Uri actual = gbu.ImageUri;

                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail("creating the event failed");
            }
        }

        [Test]
        public void GBUpcomingEvent_TryCreate_CompareToThrowsArgNullWhenParamNull()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_OneEvent.json");
            
            JObject json = JObject.Parse(testData);
            JToken token = json["upcoming"].First;
            
            if (GBUpcomingEvent.TryCreate(token, out GBUpcomingEvent gbu))
            {
                Assert.Throws<ArgumentNullException>(() => gbu.CompareTo(null));
            }
            else
            {
                Assert.Fail("creating the event failed");
            }
        }

        [Test]
        public async Task GBUpcomingEvent_CompareTo_OrdersOlderFirst()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_FiveEvents.json");
            
            var vm = new MainWindowViewModel(dummySettings, new TestWebRetriever(testData));
            
            await vm.UpdateAsync();

            // Beastcast should sort before Dead Cells
            
            var gbu1 = vm.Events.Single(x => x.Title.Contains("Beastcast"));
            var gbu2 = vm.Events.Single(x => x.Title.Contains("Dead Cells"));

            int expected1Before2 = -1;
            int actual1Before2 = gbu1.CompareTo(gbu2);

            int expected2Before1 = 1;
            int actual2Before1 = gbu2.CompareTo(gbu1);

            Assert.AreEqual(expected1Before2, actual1Before2);
            Assert.AreEqual(expected2Before1, actual2Before1);
        }

        [Test]
        public void GBUpcomingEvent_Equals_FalseWhenOtherIsNull()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_OneEvent.json");

            JObject json = JObject.Parse(testData);
            JToken token = json["upcoming"].First;

            if (GBUpcomingEvent.TryCreate(token, out GBUpcomingEvent gbu))
            {
                bool expected = false;
                bool actual = gbu.Equals(null);

                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail("creating the event failed");
            }
        }

        [Test]
        public void GBUpcomingEvent_Equals_TrueWhenAreIdentical()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_TwoEqualEvents.json");

            JObject json = JObject.Parse(testData);

            bool one = GBUpcomingEvent.TryCreate(json["upcoming"][0], out GBUpcomingEvent gbu1);
            bool two = GBUpcomingEvent.TryCreate(json["upcoming"][1], out GBUpcomingEvent gbu2);

            if (one && two)
            {
                bool expected = true;
                bool actual = gbu1.Equals(gbu2);

                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail("creating the events failed");
            }
        }

        [Test]
        public async Task GBUpcomingEvent_Equals_FalseWhenDifferent()
        {
            string testData = Utilities.LoadTextFromFile("GBJson_Live_FiveEvents.json");

            var vm = new MainWindowViewModel(dummySettings, new TestWebRetriever(testData));

            await vm.UpdateAsync();
            
            var list = vm.Events.ToList();

            bool equalityFound = false;
            for (int i = 0; i < list.Count; i++)
            {
                var gbu = list[i];
                
                equalityFound = list.Where(x => x.Equals(gbu)).Count() > 1;

                if (equalityFound)
                {
                    break;
                }
            }

            Assert.IsFalse(equalityFound);
        }
    }
}
