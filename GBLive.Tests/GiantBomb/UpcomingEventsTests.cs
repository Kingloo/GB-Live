using System;
using GBLive.WPF.GiantBomb;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace GBLive.Tests.GiantBomb
{
    [TestFixture]
    public class UpcomingEventsTests
    {
        private readonly string title = "a title";
        private readonly DateTimeOffset date = DateTimeOffset.Now;
        private readonly bool isPremium = false;
        private readonly string eventType = "an event type";
        private readonly Uri image = Settings.FallbackImage;

        [Test]
        public void Ctor_Properties_SetCorrectly()
        {
            var ue = new UpcomingEvent(title, date, isPremium, eventType, image);

            Assert.AreEqual(title, ue.Title);
            Assert.AreEqual(date, ue.Time);
            Assert.AreEqual(isPremium, ue.IsPremium);
            Assert.AreEqual(eventType, ue.EventType);
            Assert.AreEqual(image, ue.Image);
        }

        [TestCase("{\"type\":\"Video\",\"title\":\"Destiny 2\",\"image\":\"https://static.giantbomb.com\\/uploads\\/original\\/34\\/343190\\/3021892-cp_destiny2dlc_05182018.00_00_31_29.still001.jpg\",\"date\":\"May 22, 2018 06:00 AM\",\"premium\":false}")]
        public void TryCreate_GoodJson_Succeeds(string rawJson)
        {
            JObject json = JObject.Parse(rawJson);

            Assert.IsTrue(UpcomingEvent.TryCreate(json, out UpcomingEvent _));
        }

        [TestCase("{}")]
        [TestCase("{\"title\":\"Destiny 2\",\"image\":\"static.giantbomb.com/001.jpg\",\"date\":\"May 22, 2018 06:00 AM\",\"premium\":false}")]
        [TestCase("{\"type\":\"Video\",\"image\":\"static.giantbomb.com/001.jpg\",\"date\":\"May 22, 2018 06:00 AM\",\"premium\":false}")]
        [TestCase("{\"type\":\"Video\",\"title\":\"Destiny 2\",\"date\":\"May 22, 2018 06:00 AM\",\"premium\":false}")]
        [TestCase("{\"type\":\"Video\",\"title\":\"Destiny 2\",\"image\":\"static.giantbomb.com/001.jpg\",\"premium\":false}")]
        [TestCase("{\"type\":\"Video\",\"title\":\"Destiny 2\",\"image\":\"static.giantbomb.com/001.jpg\",\"date\":\"May 22, 2018 06:00 AM\"}")]
        public void TryCreate_BadJson_Fails(string rawJson)
        {
            JObject json = JObject.Parse(rawJson);

            Assert.IsFalse(UpcomingEvent.TryCreate(json, out UpcomingEvent ue));
            Assert.IsNull(ue);
        }

        [Test]
        public void TryCreate_Null_Fails()
        {
            Assert.IsFalse(UpcomingEvent.TryCreate(null, out UpcomingEvent _));
        }

        [Test]
        public void IEquatable_IdenticalEvents_True()
        {
            var event1 = new UpcomingEvent(title, date, isPremium, eventType, image);
            var event2 = new UpcomingEvent(title, date, isPremium, eventType, image);

            Assert.AreEqual(event1, event2);
        }

        [Test]
        public void IEquatable_TitleIsDifferent_False()
        {
            var event1 = new UpcomingEvent(title, date, isPremium, eventType, image);
            var event2 = new UpcomingEvent("fish", date, isPremium, eventType, image);

            Assert.AreNotEqual(event1, event2);
        }

        [Test]
        public void IEquatable_DateIsDifferent_False()
        {
            var event1 = new UpcomingEvent(title, date, isPremium, eventType, image);
            var event2 = new UpcomingEvent(title, DateTimeOffset.MinValue, isPremium, eventType, image);

            Assert.AreNotEqual(event1, event2);
        }

        [Test]
        public void IEquatable_IsPremiumIsDifferent_False()
        {
            var event1 = new UpcomingEvent(title, date, isPremium, eventType, image);
            var event2 = new UpcomingEvent(title, date, true, eventType, image);

            Assert.AreNotEqual(event1, event2);
        }

        [Test]
        public void IEquatable_EventTypeDifferent_False()
        {
            var event1 = new UpcomingEvent(title, date, isPremium, eventType, image);
            var event2 = new UpcomingEvent(title, date, isPremium, "weird event", image);

            Assert.AreNotEqual(event1, event2);
        }

        [Test]
        public void IEquatable_ImageDifferent_False()
        {
            var event1 = new UpcomingEvent(title, date, isPremium, eventType, image);
            var event2 = new UpcomingEvent(title, date, isPremium, eventType, new Uri("https://bing.com"));

            Assert.AreNotEqual(event1, event2);
        }

        [Test]
        public void ObjectEquals_NotAUpcomingEvent_False()
        {
            var event1 = new UpcomingEvent(title, date, isPremium, eventType, image);

            Assert.IsFalse(event1.Equals(new object()));
        }

        [Test]
        public void CompareTo_SoonerComesBefore()
        {
            var event1 = new UpcomingEvent(title, date, isPremium, eventType, image);
            var event2 = new UpcomingEvent(title, date.AddMinutes(2d), isPremium, eventType, image);

            int expected = -1;
            int actual = event1.CompareTo(event2);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CompareTo_LaterComesAfter()
        {
            var event1 = new UpcomingEvent(title, date.AddMinutes(1d), isPremium, eventType, image);
            var event2 = new UpcomingEvent(title, date, isPremium, eventType, image);

            int expected = 1;
            int actual = event1.CompareTo(event2);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CompareTo_SameTimeButEverythingElseDifferent_ReturnZero()
        {
            var event1 = new UpcomingEvent(title, date, isPremium, eventType, image);
            var event2 = new UpcomingEvent("fish", date, true, "something else", image);

            Assert.Zero(event1.CompareTo(event2));
        }
    }
}
