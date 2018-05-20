using System;
using System.Collections.Generic;
using GBLive.WPF.GiantBomb;
using NUnit.Framework;

namespace GBLive.Tests.GiantBomb
{
    [TestFixture]
    public class UpcomingResponseTests
    {
        [Test]
        public void Properties_DefaultValues()
        {
            var response = new UpcomingResponse();

            Assert.IsFalse(response.IsSuccessful);
            Assert.IsNotNull(response.Events);
            Assert.Zero(response.Events.Count);
            Assert.IsFalse(response.IsLive);
        }

        [TestCase(true, Reason.Success, true)]
        [TestCase(false, Reason.StringEmpty, false)]
        public void Ctor_Properties_SuccessAndReasonAndIsLive(bool isSuccess, Reason reason, bool isLive)
        {
            var response = new UpcomingResponse(isSuccess, reason, isLive);

            Assert.AreEqual(isSuccess, response.IsSuccessful);
            Assert.AreEqual(reason, response.Reason);
        }

        [TestCase(true, Reason.ParseFailed, true)]
        [TestCase(false, Reason.None, false)]
        public void Ctor_Properties_SuccessAndReasonAndIsLiveAndList(bool isSuccess, Reason reason, bool isLive)
        {
            var events = new List<UpcomingEvent>()
            {
                null, null, null
            };

            var response = new UpcomingResponse(isSuccess, reason, isLive, events);

            Assert.AreEqual(isSuccess, response.IsSuccessful);
            Assert.AreEqual(reason, response.Reason);

            Assert.IsNotNull(response.Events);
            Assert.AreEqual(events.Count, response.Events.Count);
        }

        [Test]
        public void Ctor_ListIsNull_ThrowsArgNullExc()
        {
            Assert.Throws<ArgumentNullException>(() => new UpcomingResponse(true, Reason.None, false, null));
        }
    }
}
