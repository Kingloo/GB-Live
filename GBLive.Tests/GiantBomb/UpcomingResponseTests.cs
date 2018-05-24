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

        [TestCase(true, Reason.ParseFailed, true)]
        [TestCase(false, Reason.None, false)]
        public void Ctor_Properties_SuccessAndReasonAndIsLiveAndList(bool isSuccess, Reason reason, bool isLive)
        {
            var response = new UpcomingResponse(isSuccess, reason, isLive);

            Assert.AreEqual(isSuccess, response.IsSuccessful);
            Assert.AreEqual(reason, response.Reason);
            Assert.AreEqual(isLive, response.IsLive);

            Assert.IsNotNull(response.Events);
            Assert.Zero(response.Events.Count);
        }
    }
}
